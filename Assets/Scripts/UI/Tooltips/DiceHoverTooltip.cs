using System;
using _Scripts.Dices;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Tooltips
{
    // One tooltip is shared by all dice in the game scope.
    public sealed class DiceHoverTooltip : IDisposable
    {
        private readonly Camera _camera;
        private readonly CharmTooltipView _styleSource;
        private RectTransform _canvasRect;
        private RectTransform _panel;
        private Canvas _canvas;
        private CanvasGroup _group;
        private TMP_Text _value;
        private TMP_Text _bonus;
        private TMP_Text _wear;
        private Dice _owner;
        private Tween _fade;

        public DiceHoverTooltip(Camera camera, CharmTooltipView styleSource)
        {
            _camera = camera;
            _styleSource = styleSource;
        }

        public void Show(Dice dice)
        {
            if (_panel == null) CreateView();
            if (_owner != dice)
            {
                _owner = dice;
                _fade?.Kill();
                _panel.gameObject.SetActive(true);
                _panel.SetAsLastSibling();
                _fade = _group.DOFade(1f, 0.12f).SetUpdate(true);
            }

            var data = dice.Data;
            _value.text = data.RolledValue.ToString();
            _bonus.text = data.RolledFaceBonus.ToString("+0;-0;+0");
            _wear.text = data.DamageLevel > 0 ? $"-{data.DamageLevel}" : "0";
            UpdatePosition(dice.transform.position);
        }

        public void Hide(Dice dice)
        {
            if (_owner != dice) return;
            _owner = null;
            _fade?.Kill();
            _fade = _group.DOFade(0f, 0.12f).SetUpdate(true)
                .OnComplete(() => _panel.gameObject.SetActive(false));
        }

        private void UpdatePosition(Vector3 worldPosition)
        {
            var screen = _camera.WorldToScreenPoint(worldPosition);
            var uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
            if (screen.z <= 0 || !RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, uiCamera, out var point))
                return;

            var bounds = _canvasRect.rect;
            var size = _panel.rect.size;
            const float gap = 55f;
            var x = point.x + gap + size.x * 0.5f;
            if (x + size.x * 0.5f > bounds.xMax) x = point.x - gap - size.x * 0.5f;
            x = Mathf.Clamp(x, bounds.xMin + size.x * 0.5f, bounds.xMax - size.x * 0.5f);
            var y = Mathf.Clamp(point.y + size.y * 0.5f, bounds.yMin + size.y * 0.5f, bounds.yMax - size.y * 0.5f);
            _panel.localPosition = new Vector3(x, y, 0f);
        }

        private void CreateView()
        {
            _canvas = _styleSource.transform.parent.GetComponentInParent<Canvas>(true).rootCanvas;
            _canvasRect = (RectTransform)_canvas.transform;
            var root = new GameObject("Dice Hover Tooltip", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            _panel = (RectTransform)root.transform;
            _panel.SetParent(_canvasRect, false);
            _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(0.5f, 0.5f);
            _panel.sizeDelta = new Vector2(260f, 150f);
            _group = root.GetComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
            var background = root.GetComponent<Image>();
            background.color = new Color(0.075f, 0.045f, 0.095f, 0.96f);
            background.raycastTarget = false;

            CreateImage("Header", _panel, new Vector2(0f, 56f), new Vector2(260f, 38f), new Color(0.32f, 0.22f, 0.40f));
            CreateText("Title", "КУБИК", new Vector2(0f, 56f), new Vector2(236f, 34f), 23f, TextAlignmentOptions.Center, Color.white);
            CreateText("Value label", "Значение", new Vector2(-48f, 20f), new Vector2(150f, 28f), 19f, TextAlignmentOptions.Left, Color.white);
            CreateText("Bonus label", "Бонус грани", new Vector2(-48f, -17f), new Vector2(150f, 28f), 19f, TextAlignmentOptions.Left, Color.white);
            CreateText("Wear label", "Износ", new Vector2(-48f, -53f), new Vector2(150f, 28f), 19f, TextAlignmentOptions.Left, Color.white);
            _value = CreateText("Value", "0", new Vector2(85f, 20f), new Vector2(60f, 28f), 21f, TextAlignmentOptions.Center, Color.white);
            _bonus = CreateText("Bonus", "+0", new Vector2(85f, -17f), new Vector2(60f, 28f), 21f, TextAlignmentOptions.Center, new Color(1f, 0.64f, 0.52f));
            CreateImage("Wear badge", _panel, new Vector2(85f, -53f), new Vector2(52f, 27f), new Color(0.32f, 0.22f, 0.40f));
            _wear = CreateText("Wear", "0", new Vector2(85f, -53f), new Vector2(52f, 27f), 20f, TextAlignmentOptions.Center, new Color(0.83f, 0.69f, 0.91f));
            root.SetActive(false);
        }

        private static void CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private TMP_Text CreateText(string name, string text, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            var rect = (RectTransform)go.transform;
            rect.SetParent(_panel, false);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.font = _styleSource.TitleText.font;
            label.fontStyle = FontStyles.Bold;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.raycastTarget = false;
            label.text = text;
            return label;
        }

        public void Dispose()
        {
            _fade?.Kill();
            if (_panel != null) UnityEngine.Object.Destroy(_panel.gameObject);
        }
    }
}
