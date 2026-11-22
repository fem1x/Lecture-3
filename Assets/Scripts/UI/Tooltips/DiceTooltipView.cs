using _Scripts.Configs;
using _Scripts.Dices;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Tooltips
{
    public sealed class DiceTooltipView : MonoBehaviour
    {
        [SerializeField] private Image _faceImage;
        [SerializeField] private TMP_Text _bonusText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private GameObject _damageIndicator;
        [SerializeField] private RectTransform _facePanel;
        [SerializeField] private RectTransform _bonusPanel;
        [SerializeField] private RectTransform _pointer;
        [SerializeField] private float _pointerEdgeOffset;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Min(0f)] private float _showDuration = 0.22f;
        [SerializeField, Range(0.05f, 1f)] private float _showStartScale = 0.76f;
        [SerializeField] private Ease _showScaleEase = Ease.OutBack;
        [SerializeField] private Ease _showFadeEase = Ease.OutQuad;
        [SerializeField, Min(0f)] private float _hideDuration = 0.15f;
        [SerializeField, Range(0.05f, 1f)] private float _hideEndScale = 0.84f;
        [SerializeField] private Ease _hideScaleEase = Ease.InQuad;
        [SerializeField] private Ease _hideFadeEase = Ease.InQuad;

        [SerializeField, Min(0f)] private float _moveDuration = 0.24f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;

        private readonly Vector3[] _corners = new Vector3[4];
        private Tween _animation;
        private Tween _moveTween;
        private Quaternion _pointerRestRotation;
        private bool _showing;
        private Vector2 _anchorPosition;
        private Vector2 _moveOffset;
        private int _lastBonus = int.MinValue;

        public Vector3 RestScale { get; private set; }
        public bool IsShowing => _showing;
        public Vector2 LayoutPosition { get; private set; }

        public void InitializeHidden()
        {
            RestScale = transform.localScale;
            _pointerRestRotation = _pointer.localRotation;
            HideImmediately();
        }

        public void SetVisible(bool visible)
        {
            if (_showing == visible) return;
            _showing = visible;
            _animation?.Kill();
            if (!visible) StopMove();

            if (visible && !gameObject.activeSelf)
            {
                transform.localScale = RestScale * _showStartScale;
                gameObject.SetActive(true);
            }

            var duration = visible ? _showDuration : _hideDuration;
            var targetScale = visible ? RestScale : RestScale * _hideEndScale;
            var targetAlpha = visible ? 1f : 0f;
            if (duration <= 0f)
            {
                transform.localScale = targetScale;
                _canvasGroup.alpha = targetAlpha;
                if (!visible) gameObject.SetActive(false);
                return;
            }

            _animation = DOTween.Sequence().SetUpdate(true)
                .Join(transform.DOScale(targetScale, duration).SetEase(visible ? _showScaleEase : _hideScaleEase))
                .Join(_canvasGroup.DOFade(targetAlpha, duration).SetEase(visible ? _showFadeEase : _hideFadeEase))
                .OnComplete(() =>
                {
                    _animation = null;
                    if (!visible) gameObject.SetActive(false);
                });
        }

        public void MoveTo(Vector2 position, bool canSlide)
        {
            if (_showing && LayoutPosition == position && (canSlide || _moveTween == null)) return;

            StopMove();
            LayoutPosition = position;
            if (_showing && !canSlide)
                HideImmediately();

            if (!_showing || _moveDuration <= 0f)
            {
                transform.localPosition = position;
                return;
            }

            _anchorPosition = position;
            _moveOffset = (Vector2)transform.localPosition - position;
            _moveTween = DOTween.To(() => _moveOffset, value =>
                {
                    _moveOffset = value;
                    transform.localPosition = _anchorPosition + _moveOffset;
                }, Vector2.zero, _moveDuration)
                .SetEase(_moveEase).SetUpdate(true)
                .OnComplete(() => _moveTween = null);
        }

        public void Follow(Vector2 delta)
        {
            LayoutPosition += delta;
            _anchorPosition += delta;
            transform.localPosition += (Vector3)delta;
        }

        private void StopMove()
        {
            _moveTween?.Kill();
            _moveTween = null;
        }

        public void HideImmediately()
        {
            _showing = false;
            StopMove();
            _animation?.Kill();
            _animation = null;
            _canvasGroup.alpha = 0f;
            transform.localScale = RestScale;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            StopMove();
            _animation?.Kill();
        }

        public Rect GetBodyBounds() => GetLocalBounds(_bonusPanel);

        public Rect GetLayoutBounds(Rect body)
        {
            var bounds = body;
            var pointerReach = _pointer.rect.size.magnitude * 0.5f + Mathf.Abs(_pointerEdgeOffset);
            return Rect.MinMaxRect(bounds.xMin - pointerReach, bounds.yMin - pointerReach,
                bounds.xMax + pointerReach, bounds.yMax + pointerReach);
        }

        public void PointAt(Vector2 target, Rect body)
        {
            var offset = target - body.center;
            var halfSize = body.size * 0.5f;
            var horizontal = Mathf.Abs(offset.x) * halfSize.y > Mathf.Abs(offset.y) * halfSize.x;
            var margin = Mathf.Min(_pointer.rect.width * 0.5f, Mathf.Min(halfSize.x, halfSize.y));
            Vector2 position;
            float angle;
            if (horizontal)
            {
                position = new Vector2(offset.x < 0 ? body.xMin - _pointerEdgeOffset : body.xMax + _pointerEdgeOffset,
                    Mathf.Clamp(target.y, body.yMin + margin, body.yMax - margin));
                angle = offset.x < 0 ? 0f : 180f;
            }
            else
            {
                position = new Vector2(Mathf.Clamp(target.x, body.xMin + margin, body.xMax - margin),
                    offset.y < 0 ? body.yMin - _pointerEdgeOffset : body.yMax + _pointerEdgeOffset);
                angle = offset.y < 0 ? 90f : -90f;
            }

            _pointer.localPosition = position;
            _pointer.localRotation = Quaternion.Euler(0f, 0f, angle) * _pointerRestRotation;
        }

        private Rect GetLocalBounds(RectTransform element)
        {
            element.GetWorldCorners(_corners);
            var min = (Vector2)transform.InverseTransformPoint(_corners[0]);
            var max = min;
            for (var i = 1; i < _corners.Length; i++)
            {
                var point = (Vector2)transform.InverseTransformPoint(_corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static Rect Union(Rect a, Rect b) => Rect.MinMaxRect(
            Mathf.Min(a.xMin, b.xMin), Mathf.Min(a.yMin, b.yMin),
            Mathf.Max(a.xMax, b.xMax), Mathf.Max(a.yMax, b.yMax));

        public void Refresh(DiceData data, DiceAtlasConfig atlas)
        {
            var bonus = data.RolledFaceBonus;
            if (_lastBonus == bonus) return;
            _lastBonus = bonus;
            _bonusText.text = $"{bonus:+0;-0;+0} оч.";
        }
    }
}
