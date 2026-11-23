using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ButtonScaleView : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;
        [SerializeField] private bool _insideLayoutGroup;

        [Header("Scale & Squash")]
        [SerializeField] private float _hoverScale = 1.08f;
        [SerializeField] private float _pressedScale = 0.92f;
        [SerializeField, Range(0f, 0.35f)] private float _squashAmount = 0.08f;

        [Header("Tactile Offset & Tilt")]
        [SerializeField] private float _pressDipY = 4f;
        [SerializeField] private float _hoverTiltZ = 0f;
        [SerializeField] private float _pressTiltZ = 0f;

        [Header("Timings & Eases")]
        [SerializeField] private float _hoverDuration = 0.15f;
        [SerializeField] private Ease _hoverEase = Ease.OutQuart;
        [Space]
        [SerializeField] private float _pressDuration = 0.06f;
        [SerializeField] private Ease _pressEase = Ease.OutQuad;
        [Space]
        [SerializeField] private float _releaseDuration = 0.25f;
        [SerializeField] private Ease _releaseEase = Ease.OutBack;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private RectTransform _rectTransform;
        private Vector3 _defaultScale;
        private Vector2 _defaultAnchoredPos;
        private float _defaultZRot;

        private Sequence _feedbackSeq;
        private bool _isHovered;
        private bool _isPressed;
        private int _tiltSign = 1;
        private RectTransform _pressHitArea;
        private Vector3 _pressHitPosition;
        private Quaternion _pressHitRotation;
        private Vector3 _pressHitScale;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Reset() => _button = GetComponent<Button>();

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;

            if (!_insideLayoutGroup)
                CenterPivot();

            _defaultScale = transform.localScale;
            _defaultAnchoredPos = _rectTransform.anchoredPosition;
            _defaultZRot = _rectTransform.localEulerAngles.z;
        }

        private void OnDisable()
        {
            _feedbackSeq?.Kill();
            _isHovered = false;
            _isPressed = false;
            if (_pressHitArea != null)
                _pressHitArea.gameObject.SetActive(false);

            transform.localScale = _defaultScale;
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _defaultZRot);

            if (!_insideLayoutGroup)
                _rectTransform.anchoredPosition = _defaultAnchoredPos;
        }

        private void OnDestroy() => _feedbackSeq?.Kill();

        private void LateUpdate()
        {
            if (!_isPressed) return;

            _pressHitArea.SetPositionAndRotation(_pressHitPosition, _pressHitRotation);
            var parentScale = _rectTransform.lossyScale;
            _pressHitArea.localScale = new Vector3(
                _pressHitScale.x / parentScale.x,
                _pressHitScale.y / parentScale.y,
                _pressHitScale.z / parentScale.z);
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            PlayFeedback(isRelease: false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            PlayFeedback(isRelease: true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            PreparePressHitArea();
            _isPressed = true;
            _tiltSign = -_tiltSign;
            PlayFeedback(isRelease: false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;

            _isPressed = false;
            _pressHitArea.gameObject.SetActive(false);
            PlayFeedback(isRelease: true);
        }

        private void PreparePressHitArea()
        {
            if (_pressHitArea == null)
            {
                var hitArea = new GameObject("Press Hit Area", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                hitArea.layer = gameObject.layer;
                _pressHitArea = (RectTransform)hitArea.transform;
                _pressHitArea.SetParent(_rectTransform, false);
                var image = hitArea.GetComponent<Image>();
                image.color = Color.clear;
            }

            _pressHitArea.anchorMin = _rectTransform.pivot;
            _pressHitArea.anchorMax = _rectTransform.pivot;
            _pressHitArea.pivot = _rectTransform.pivot;
            _pressHitArea.anchoredPosition = Vector2.zero;
            _pressHitArea.sizeDelta = _rectTransform.rect.size;
            _pressHitArea.localRotation = Quaternion.identity;
            _pressHitArea.localScale = Vector3.one;
            _pressHitPosition = _pressHitArea.position;
            _pressHitRotation = _pressHitArea.rotation;
            _pressHitScale = _pressHitArea.lossyScale;
            _pressHitArea.gameObject.SetActive(true);
        }

        private void CenterPivot()
        {
            var center = new Vector2(0.5f, 0.5f);
            var deltaPivot = _rectTransform.pivot - center;
            var deltaPosition = new Vector2(
                deltaPivot.x * _rectTransform.rect.width * _rectTransform.localScale.x,
                deltaPivot.y * _rectTransform.rect.height * _rectTransform.localScale.y);

            _rectTransform.pivot = center;
            _rectTransform.anchoredPosition -= deltaPosition;
        }

        private void PlayFeedback(bool isRelease)
        {
            var canInteract = _button.interactable;

            var targetScale = GetTargetScale(canInteract);

            var targetRotZ = _defaultZRot + ((_isPressed && canInteract)
                ? _tiltSign * _pressTiltZ
                : (_isHovered && canInteract)
                    ? _tiltSign * _hoverTiltZ * 0.5f
                    : 0f);

            var duration = (_isPressed && canInteract)
                ? _pressDuration
                : isRelease
                    ? _releaseDuration
                    : _hoverDuration;

            var ease = (_isPressed && canInteract)
                ? _pressEase
                : isRelease
                    ? _releaseEase
                    : _hoverEase;

            _feedbackSeq?.Kill();
            _feedbackSeq = DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject)
                .Join(transform.DOScale(targetScale, duration).SetEase(ease))
                .Join(_rectTransform.DOLocalRotate(new Vector3(0f, 0f, targetRotZ), duration).SetEase(ease));

            if (!_insideLayoutGroup)
            {
                var targetPos = (_isPressed && canInteract)
                    ? _defaultAnchoredPos + new Vector2(0f, -_pressDipY)
                    : _defaultAnchoredPos;

                _feedbackSeq.Join(_rectTransform.DOAnchorPos(targetPos, duration).SetEase(ease));
            }
        }

        private Vector3 GetTargetScale(bool canInteract)
        {
            if (_isPressed && canInteract)
            {
                var squashX = _defaultScale.x * (_pressedScale + _squashAmount);
                var squashY = _defaultScale.y * Mathf.Max(0.1f, _pressedScale - _squashAmount);
                return new Vector3(squashX, squashY, _defaultScale.z);
            }

            if (_isHovered && canInteract)
                return _defaultScale * _hoverScale;

            return _defaultScale;
        }
    }
}
