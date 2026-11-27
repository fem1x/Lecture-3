using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.Charms
{
    [RequireComponent(typeof(Image))]
    public class CharmScrapView : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Highlight")]
        [SerializeField] private Color _dragTint = new(1f, 0.72f, 0.35f);
        [SerializeField] private Color _hoverTint = new(1f, 0.52f, 0.15f);

        [Header("Charm Motion")]
        [SerializeField, Min(0.05f)] private float _pullDuration = 0.22f;
        [SerializeField] private Ease _pullEase = Ease.InCubic;
        [SerializeField] private Ease _shrinkEase = Ease.InBack;
        [SerializeField, Range(0f, 0.95f), Tooltip("Fraction of Pull Duration elapsed before the charm starts fading.")]
        private float _fadeStart = 0.4f;

        [Header("Pull Shake")]
        [SerializeField, Min(0f), Tooltip("Maximum displacement in UI units. Use 0 to disable.")]
        private float _pullShakeStrength = 5f;
        [SerializeField, Min(0f), Tooltip("Oscillations per second. Use 0 to disable.")]
        private float _pullShakeFrequency = 26f;
        [SerializeField, Tooltip("Fraction of shake strength over the normalized pull duration.")]
        private AnimationCurve _pullShakeGrowth = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Charm Rotation")]
        [SerializeField, Tooltip("Signed rotation in degrees around the charm's local Z axis. Use 0 to disable spin.")]
        private float _rotationAngle = -24f;
        [SerializeField] private Ease _rotationEase = Ease.OutQuad;
        [SerializeField, Range(0.05f, 1f), Tooltip("Fraction of Pull Duration used for the rotation.")]
        private float _rotationDurationFraction = 1f;

        [Header("Impact")]
        [SerializeField, Min(0.05f)] private float _impactDuration = 0.36f;
        [SerializeField] private Color _impactTint = new(1f, 0.83f, 0.4f);
        [SerializeField, Range(0f, 0.5f)] private float _impactScale = 0.12f;
        [Header("Metal Fragments")]
        [SerializeField, Range(0, 24), FormerlySerializedAs("_sparkCount")] private int _fragmentCount = 10;
        [SerializeField, Min(0f), FormerlySerializedAs("_sparkDistance")] private float _fragmentDistance = 70f;
        [SerializeField, Min(0.1f), FormerlySerializedAs("_sparkSpeedMultiplier")]
        [Tooltip("Fragment flight speed multiplier. Higher values shorten flight time.")]
        private float _fragmentSpeedMultiplier = 1f;
        [SerializeField, Min(0.1f), FormerlySerializedAs("_sparkSizeMultiplier")] private float _fragmentSizeMultiplier = 1f;
        [SerializeField] private Color _fragmentPrimaryColor = new Color32(163, 174, 184, 255);
        [SerializeField] private Color _fragmentSecondaryColor = new Color32(197, 205, 211, 255);

        private CharmsConveyorService _conveyorService;
        private CharmDragState _dragState;
        private ISfxPlayer _sfxPlayer;
        private Image _image;
        private Color _restColor;
        private Vector3 _restScale;
        private bool _isHovered;
        private Tween _highlightTween;
        private Sequence _impactTween;
        private GameObject _fragmentRoot;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(CharmsConveyorService conveyorService, CharmDragState dragState, ISfxPlayer sfxPlayer)
        {
            _conveyorService = conveyorService;
            _dragState = dragState;
            _sfxPlayer = sfxPlayer;
            _dragState.OnDragChanged += HandleDragChanged;
        }

        private void Awake() => _image = GetComponent<Image>();

        private void OnEnable()
        {
            _restColor = _image.color;
            _restScale = transform.localScale;
        }

        private void OnDisable()
        {
            _highlightTween?.Kill();
            _impactTween?.Kill();
            ClearFragments();
            _isHovered = false;
            _image.color = _restColor;
            transform.localScale = _restScale;
        }

        private void OnDestroy()
        {
            if (_dragState != null)
                _dragState.OnDragChanged -= HandleDragChanged;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (_dragState.IsDragging)
                HandleDragChanged(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (_dragState.IsDragging)
                HandleDragChanged(true);
        }

        private void HandleDragChanged(bool isDragging)
        {
            if (!isActiveAndEnabled || _impactTween?.IsActive() == true) return;

            var tint = _isHovered ? WithRestAlpha(_hoverTint) : Color.Lerp(_restColor, WithRestAlpha(_dragTint), 0.5f);
            _highlightTween?.Kill();
            _highlightTween = _image.DOColor(isDragging ? tint : _restColor, 0.12f)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<CharmDragView>(out var dragView)) return;
            if (!dragView.IsDraggedBy(eventData.pointerId)) return;

            if (!_conveyorService.TryScrap(dragView.GeneralView.Config)) return;

            _sfxPlayer.PlayUI("CharmScrapPull", (RectTransform)dragView.transform);
            var target = _image.rectTransform.TransformPoint(_image.rectTransform.rect.center);
            dragView.AnimateScrap(
                target,
                _pullDuration,
                _pullEase,
                (_pullShakeStrength, _pullShakeFrequency, _pullShakeGrowth),
                _shrinkEase,
                _pullDuration * _fadeStart,
                _rotationAngle,
                _pullDuration * _rotationDurationFraction,
                _rotationEase);
            PlayImpact();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void PlayImpact()
        {
            _highlightTween?.Kill();
            _impactTween?.Kill();
            ClearFragments();
            _image.color = WithRestAlpha(_hoverTint);

            var flashDuration = _impactDuration * 0.2f;
            _impactTween = DOTween.Sequence()
                .InsertCallback(_pullDuration, () => _sfxPlayer.PlayUI("CharmShatter", _image.rectTransform))
                .Insert(_pullDuration, _image.DOColor(WithRestAlpha(_impactTint), flashDuration))
                .Insert(_pullDuration + flashDuration, _image.DOColor(_restColor, _impactDuration - flashDuration))
                .Insert(_pullDuration, transform.DOPunchScale(_restScale * _impactScale, _impactDuration, 4, 0.5f))
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(ClearFragments);

            if (_fragmentCount > 0)
                AddFragments(_impactTween);
        }

        private void AddFragments(Sequence sequence)
        {
            _fragmentRoot = new GameObject("Metal Fragments", typeof(RectTransform), typeof(LayoutElement));
            var root = (RectTransform)_fragmentRoot.transform;
            root.SetParent(transform, false);
            root.anchorMin = root.anchorMax = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = Vector2.zero;
            _fragmentRoot.GetComponent<LayoutElement>().ignoreLayout = true;

            var random = new System.Random();
            for (var i = 0; i < _fragmentCount; i++)
            {
                var spread = (i + (float)random.NextDouble()) / _fragmentCount;
                var radians = Mathf.Lerp(-0.12f, 1.12f, spread) * Mathf.PI;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                var fragment = new GameObject(
                        "Metal Fragment", typeof(RectTransform), typeof(CanvasRenderer), typeof(ScrapFragmentGraphic))
                    .GetComponent<ScrapFragmentGraphic>();
                var rect = fragment.rectTransform;
                rect.SetParent(root, false);
                fragment.raycastTarget = false;
                fragment.SetShape(i);
                fragment.color = i % 2 == 0 ? _fragmentPrimaryColor : _fragmentSecondaryColor;
                fragment.enabled = false;

                var width = Mathf.Lerp(5f, 12f, (float)random.NextDouble());
                var height = Mathf.Lerp(4f, 9f, (float)random.NextDouble());
                rect.sizeDelta = new Vector2(width, height) * _fragmentSizeMultiplier;

                var travel = direction * (_fragmentDistance * Mathf.Lerp(0.4f, 1f, (float)random.NextDouble()));
                var drop = Vector2.down * (_fragmentDistance * Mathf.Lerp(0.6f, 0.9f, (float)random.NextDouble()));
                var startAngle = (float)random.NextDouble() * 360f;
                var spin = Mathf.Lerp(110f, 290f, (float)random.NextDouble()) * (i % 2 == 0 ? 1f : -1f);
                var flipPhase = (float)random.NextDouble() * 2f * Mathf.PI;
                var flipRate = Mathf.Lerp(4f, 10f, (float)random.NextDouble());
                var delay = _impactDuration * (float)random.NextDouble() * 0.08f;
                var life = _impactDuration * Mathf.Lerp(0.8f, 1.35f, (float)random.NextDouble())
                    / _fragmentSpeedMultiplier;

                sequence.Insert(_pullDuration + delay, DOVirtual.Float(0f, 1f, life, progress =>
                {
                    rect.anchoredPosition = travel * progress + drop * (progress * progress);
                    rect.localRotation = Quaternion.Euler(0f, 0f, startAngle + spin * progress);
                    var face = Mathf.Cos(flipPhase + flipRate * progress);
                    var turnScale = Mathf.Lerp(0.24f, 1f, Mathf.Abs(face));
                    var shrink = Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.58f, 1f, progress));
                    rect.localScale = new Vector3(turnScale * shrink, shrink, 1f);
                    var fade = Mathf.InverseLerp(0.8f, 1f, progress);
                    fragment.enabled = true;
                    fragment.canvasRenderer.SetAlpha(1f - fade);
                }).SetEase(Ease.Linear));
            }
        }

        private void ClearFragments()
        {
            if (_fragmentRoot == null) return;

            Destroy(_fragmentRoot);
            _fragmentRoot = null;
        }

        private Color WithRestAlpha(Color color)
        {
            color.a = _restColor.a;
            return color;
        }
    }
}
