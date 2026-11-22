using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace _Scripts.UI.Tooltips
{
    public sealed class DiceHoverTooltip : System.IDisposable
    {
        private const float Gap = 8f;
        private const float ScreenMargin = 6f;
        private const float UiMargin = 8f;
        private const float EdgeTolerance = 8f;
        private const float RepositionDistance = 32f;
        private const float OverlapTolerance = 2f;

        private readonly Camera _camera;
        private readonly Canvas _canvas;
        private readonly RectTransform _holder;
        private readonly RectTransform[] _uiObstacles;
        private readonly DiceAtlasConfig _atlas;
        private readonly DiceTableController _table;
        private readonly GameLifetimeScope _scope;
        private readonly List<(Dice Dice, Rect Bounds)> _projectedDice = new();
        private readonly List<Rect> _placedTooltips = new();
        private readonly List<Rect> _damageIndicatorBounds = new();
        private readonly List<Rect> _blockedUiRects = new();
        private readonly Vector3[] _uiCorners = new Vector3[4];
        private sealed class Placement
        {
            public bool Valid;
            public Vector2 LastScreenCenter;
            public Vector3 LastWorldCenter;
            public Vector2 Offset;
            public int LastDamage = int.MinValue;
            public float LastOverlap;
        }

        private readonly Dictionary<Dice, Placement> _placements = new();
        private readonly List<Rect> _previousUiRects = new();
        private bool _obstaclesChanged;
        private Rect _previousHolderBounds;
        private int _layoutFrame = -1;

        public bool AutoShow => _scope.AutoShowDiceTooltips;

        public DiceHoverTooltip(Camera camera, RectTransform holder, RectTransform[] uiObstacles,
            DiceAtlasConfig atlas, DiceTableController table, GameLifetimeScope scope)
        {
            _camera = camera;
            _holder = holder;
            _uiObstacles = uiObstacles;
            _canvas = holder.GetComponentInParent<Canvas>(true).rootCanvas;
            _atlas = atlas;
            _table = table;
            _scope = scope;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            Camera.onPreCull += OnPreCull;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _camera) UpdateLayout();
        }

        private void OnPreCull(Camera camera)
        {
            if (GraphicsSettings.currentRenderPipeline == null && camera == _camera) UpdateLayout();
        }

        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            Camera.onPreCull -= OnPreCull;
            _placements.Clear();
        }

        public void Remove(Dice dice) => _placements.Remove(dice);

        public DiceTooltipView Create(DiceTooltipView prefab, int diceId)
        {
            var view = Object.Instantiate(prefab, _holder, false);
            view.gameObject.name = $"Dice Tooltip ({diceId})";
            foreach (var graphic in view.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            view.InitializeHidden();
            return view;
        }

        public DiceDamageIndicatorView CreateDamageIndicator(TMPro.TMP_Text prefab, int diceId)
        {
            var indicator = Object.Instantiate(prefab.transform.root.gameObject, _holder, false);
            indicator.name = $"Dice Damage ({diceId})";
            foreach (var graphic in indicator.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            var view = indicator.GetComponent<DiceDamageIndicatorView>();
            view.InitializeHidden();
            return view;
        }

        public void UpdateLayout()
        {
            if (_layoutFrame == Time.frameCount) return;
            _layoutFrame = Time.frameCount;
            _projectedDice.Clear();
            _placedTooltips.Clear();
            _damageIndicatorBounds.Clear();
            UpdateUiObstacles();

            var holderBounds = _holder.rect;
            var edgeToleranceBounds = Rect.MinMaxRect(holderBounds.xMin - EdgeTolerance,
                holderBounds.yMin - EdgeTolerance, holderBounds.xMax + EdgeTolerance, holderBounds.yMax + EdgeTolerance);
            var uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            foreach (var dice in _table.AllDices)
            {
                if (!dice.isActiveAndEnabled) continue;
                var visibilityBounds = dice.Tooltip.IsShowing || dice.DamageIndicator.IsShowing
                    ? edgeToleranceBounds : holderBounds;
                if (TryProjectBounds(dice.TooltipAnchorBounds, uiCamera, out var bounds) && bounds.Overlaps(visibilityBounds))
                    _projectedDice.Add((dice, bounds));
                else
                {
                    dice.Tooltip.SetVisible(false);
                    dice.DamageIndicator.SetVisible(false);
                    if (_placements.TryGetValue(dice, out var hiddenPlacement)) hiddenPlacement.Valid = false;
                }
            }

            foreach (var (dice, bounds) in _projectedDice)
            {
                if (!dice.ShouldShowTooltip || dice.Data.DamageLevel <= 0) continue;
                var size = dice.DamageIndicator.MaximumSize;
                var center = new Vector2(bounds.xMax, bounds.yMax);
                _damageIndicatorBounds.Add(new Rect(center - size * 0.5f, size));
            }

            var usableBounds = Rect.MinMaxRect(holderBounds.xMin + ScreenMargin, holderBounds.yMin + ScreenMargin,
                holderBounds.xMax - ScreenMargin, holderBounds.yMax - ScreenMargin);
            foreach (var (dice, bounds) in _projectedDice)
            {
                var view = dice.Tooltip;
                if (!_placements.TryGetValue(dice, out var placement))
                {
                    placement = new Placement();
                    _placements.Add(dice, placement);
                }
                var show = dice.ShouldShowTooltip;
                var damage = dice.DamageIndicator;
                var showDamage = show && dice.Data.DamageLevel > 0;
                damage.SetVisible(showDamage);
                if (showDamage)
                {
                    if (placement.LastDamage != dice.Data.DamageLevel)
                    {
                        placement.LastDamage = dice.Data.DamageLevel;
                        damage.SetDamage(placement.LastDamage);
                    }
                    var damageRect = (RectTransform)damage.transform;
                    damageRect.localPosition = new Vector3(bounds.xMax, bounds.yMax, 0f);
                }

                if (!show || dice.Data.RolledFaceBonus == 0)
                {
                    view.SetVisible(false);
                    placement.Valid = false;
                    continue;
                }

                view.Refresh(dice.Data, _atlas);
                var body = view.GetBodyBounds();
                var panel = (RectTransform)view.transform;
                var footprint = view.GetLayoutBounds(body);
                var scale = (Vector2)view.RestScale;
                var scaledFootprint = new Rect(Vector2.Scale(footprint.position, scale), Vector2.Scale(footprint.size, scale));
                var center = bounds.center;
                var worldCenter = dice.TooltipAnchorBounds.center;
                var desiredCenter = center + placement.Offset;
                var desiredRect = new Rect(desiredCenter - scaledFootprint.size * 0.5f, scaledFootprint.size);
                var movedFar = (worldCenter - placement.LastWorldCenter).sqrMagnitude > 0.0025f;
                var outside = desiredRect.xMin < usableBounds.xMin || desiredRect.xMax > usableBounds.xMax ||
                              desiredRect.yMin < usableBounds.yMin || desiredRect.yMax > usableBounds.yMax;
                var blocked = OverlapsUi(desiredRect) || OverlapsDamage(desiredRect);
                var overlapping = OverlapsDice(desiredRect) || GetOverlap(desiredRect) > placement.LastOverlap +
                    Mathf.Min(scaledFootprint.size.x, scaledFootprint.size.y) * OverlapTolerance;
                var recalculate = !placement.Valid || !view.IsShowing || _obstaclesChanged || movedFar || outside || blocked || overlapping;
                if (placement.Valid && view.IsShowing)
                    view.Follow(center - placement.LastScreenCenter);

                if (recalculate)
                {
                    Vector2? previous = placement.Valid ? desiredCenter : null;
                    if (!TryChoosePosition(bounds, scaledFootprint.size, usableBounds, previous, out var position))
                    {
                        view.HideImmediately();
                        placement.Valid = false;
                        continue;
                    }

                    var origin = position - scaledFootprint.center;
                    var currentCenter = (Vector2)panel.localPosition + scaledFootprint.center;
                    var halfSize = scaledFootprint.size * 0.5f;
                    var pathMin = Vector2.Min(currentCenter, position) - halfSize;
                    var pathMax = Vector2.Max(currentCenter, position) + halfSize;
                    var path = Rect.MinMaxRect(pathMin.x, pathMin.y, pathMax.x, pathMax.y);
                    var sameDirection = !placement.Valid || GetDirection(desiredCenter, bounds.center) ==
                        GetDirection(position, bounds.center);
                    view.MoveTo(origin, sameDirection && !OverlapsUi(path) && !OverlapsDice(path) && !OverlapsDamage(path));
                    placement.Offset = position - center;
                    placement.LastOverlap = GetOverlap(new Rect(position - halfSize, scaledFootprint.size));
                    placement.LastWorldCenter = worldCenter;
                    placement.Valid = true;
                }

                placement.LastScreenCenter = center;
                var target = bounds.center - (Vector2)panel.localPosition;
                view.PointAt(new Vector2(target.x / scale.x, target.y / scale.y), body);
                view.SetVisible(true);
                var actualCenter = (Vector2)panel.localPosition + scaledFootprint.center;
                _placedTooltips.Add(new Rect(actualCenter - scaledFootprint.size * 0.5f, scaledFootprint.size));
            }
        }

        private void UpdateUiObstacles()
        {
            _previousUiRects.Clear();
            _previousUiRects.AddRange(_blockedUiRects);
            _blockedUiRects.Clear();
            foreach (var obstacle in _uiObstacles)
            {
                if (!obstacle.gameObject.activeInHierarchy) continue;
                obstacle.GetWorldCorners(_uiCorners);
                var min = (Vector2)_holder.InverseTransformPoint(_uiCorners[0]);
                var max = min;
                for (var i = 1; i < _uiCorners.Length; i++)
                {
                    var point = (Vector2)_holder.InverseTransformPoint(_uiCorners[i]);
                    min = Vector2.Min(min, point);
                    max = Vector2.Max(max, point);
                }

                _blockedUiRects.Add(Rect.MinMaxRect(min.x - UiMargin, min.y - UiMargin,
                    max.x + UiMargin, max.y + UiMargin));
            }

            var holderBounds = _holder.rect;
            _obstaclesChanged = holderBounds != _previousHolderBounds || _previousUiRects.Count != _blockedUiRects.Count;
            for (var i = 0; i < _blockedUiRects.Count && !_obstaclesChanged; i++)
                _obstaclesChanged = _blockedUiRects[i] != _previousUiRects[i];
            _previousHolderBounds = holderBounds;
        }

        private bool TryChoosePosition(Rect dice, Vector2 size, Rect bounds, Vector2? previous, out Vector2 position)
        {
            var half = size * 0.5f;
            var best = bounds.center;
            var bestOverlap = float.PositiveInfinity;
            var bestDistance = float.PositiveInfinity;
            var bestSide = int.MaxValue;
            Consider(new Vector2(dice.center.x, dice.yMin - Gap - half.y));
            Consider(new Vector2(dice.xMax + Gap + half.x, dice.center.y));
            Consider(new Vector2(dice.xMin - Gap - half.x, dice.center.y));
            Consider(new Vector2(dice.center.x, dice.yMax + Gap + half.y));

            // Keep the current position through small changes in overlap or distance.
            if (previous is Vector2 current)
            {
                var rect = new Rect(current - half, size);
                var aligned = Mathf.Abs(current.x - dice.center.x) < 0.1f ||
                              Mathf.Abs(current.y - dice.center.y) < 0.1f;
                if (aligned && rect.xMin >= bounds.xMin - 0.1f && rect.xMax <= bounds.xMax + 0.1f &&
                    rect.yMin >= bounds.yMin - 0.1f && rect.yMax <= bounds.yMax + 0.1f &&
                    !OverlapsUi(rect) && !OverlapsDice(rect) && !OverlapsDamage(rect))
                {
                    var overlap = GetOverlap(rect);
                    var distance = Vector2.Distance(current, dice.center);
                    var side = rect.yMax <= dice.yMin ? 0 : 1;
                    if (side == bestSide && overlap <= bestOverlap + Mathf.Min(size.x, size.y) * OverlapTolerance &&
                        distance <= bestDistance + RepositionDistance)
                    {
                        position = current;
                        return true;
                    }
                }
            }

            position = best;
            return bestOverlap < float.PositiveInfinity;

            void Consider(Vector2 candidate)
            {
                var center = candidate;
                var rect = new Rect(center - half, size);
                if (rect.xMin < bounds.xMin || rect.xMax > bounds.xMax ||
                    rect.yMin < bounds.yMin || rect.yMax > bounds.yMax) return;
                if (OverlapsUi(rect) || OverlapsDice(rect) || OverlapsDamage(rect)) return;
                var overlap = GetOverlap(rect);
                var side = rect.yMax <= dice.yMin ? 0 : 1;
                var distance = (center - dice.center).magnitude + (center - candidate).magnitude * 2f;
                if (overlap > bestOverlap ||
                    (overlap == bestOverlap && (side > bestSide || (side == bestSide && distance >= bestDistance)))) return;
                best = center;
                bestOverlap = overlap;
                bestSide = side;
                bestDistance = distance;
            }
        }

        private bool OverlapsUi(Rect rect)
        {
            foreach (var uiRect in _blockedUiRects)
                if (rect.Overlaps(uiRect)) return true;
            return false;
        }

        private bool OverlapsDice(Rect rect)
        {
            foreach (var (_, diceBounds) in _projectedDice)
                if (rect.Overlaps(diceBounds)) return true;
            return false;
        }

        private bool OverlapsDamage(Rect rect)
        {
            foreach (var damageBounds in _damageIndicatorBounds)
                if (rect.Overlaps(damageBounds)) return true;
            return false;
        }

        private float GetOverlap(Rect rect)
        {
            var overlap = 0f;
            foreach (var (_, diceBounds) in _projectedDice)
                overlap += OverlapArea(rect, diceBounds) * 2f;
            foreach (var tooltipBounds in _placedTooltips)
                overlap += OverlapArea(rect, tooltipBounds);
            return overlap;
        }

        private bool TryProjectBounds(Bounds bounds, Camera uiCamera, out Rect result)
        {
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            var projected = false;
            for (var i = 0; i < 8; i++)
            {
                var corner = new Vector3((i & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (i & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (i & 4) == 0 ? bounds.min.z : bounds.max.z);
                var screen = _camera.WorldToScreenPoint(corner);
                if (screen.z < _camera.nearClipPlane || screen.z > _camera.farClipPlane ||
                    !RectTransformUtility.ScreenPointToLocalPointInRectangle(_holder, screen, uiCamera, out var point))
                    continue;

                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
                projected = true;
            }

            result = projected ? Rect.MinMaxRect(min.x, min.y, max.x, max.y) : default;
            return projected;
        }

        private static int GetDirection(Vector2 tooltip, Vector2 dice)
        {
            var offset = tooltip - dice;
            if (Mathf.Abs(offset.x) < 0.1f) return offset.y < 0f ? 0 : 1;
            return offset.x < 0f ? 2 : 3;
        }

        private static float OverlapArea(Rect a, Rect b) =>
            Mathf.Max(0f, Mathf.Min(a.xMax, b.xMax) - Mathf.Max(a.xMin, b.xMin)) *
            Mathf.Max(0f, Mathf.Min(a.yMax, b.yMax) - Mathf.Max(a.yMin, b.yMin));
    }
}
