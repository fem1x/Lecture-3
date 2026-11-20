using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonDisabledView : MonoBehaviour, IPointerDownHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;
        [SerializeField] private bool _insideLayoutGroup;

        [Header("Shake By X (Non-Layout)")]
        [SerializeField] private float _shakePosX = 4f;
        [SerializeField] private int _shakeVibratoX = 22;

        [Header("Shake By Z (Tilt)")]
        [SerializeField] private float _shakeAngleZ = 0.5f;
        [SerializeField] private int _shakeVibratoZ = 22;

        [Header("Timing")]
        [SerializeField] private float _duration = 0.16f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private RectTransform _rectTransform;
        private Vector2 _defaultAnchoredPos;
        private float _defaultZRot;
        private Sequence _shakeSeq;

        private void Reset() => _button = GetComponent<Button>();

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            if (_button == null)
                _button = GetComponent<Button>();

            _defaultAnchoredPos = _rectTransform.anchoredPosition;
            _defaultZRot = _rectTransform.localEulerAngles.z;
        }

        private void OnDisable()
        {
            _shakeSeq?.Kill();
            if (_rectTransform == null) return;

            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _defaultZRot);
            if (!_insideLayoutGroup)
                _rectTransform.anchoredPosition = _defaultAnchoredPos;
        }

        private void OnDestroy() => _shakeSeq?.Kill();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button.interactable) return;

            _shakeSeq?.Kill();
            _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _defaultZRot);

            if (!_insideLayoutGroup)
                _rectTransform.anchoredPosition = _defaultAnchoredPos;

            _shakeSeq = DOTween.Sequence()
                .SetUpdate(true)
                .SetLink(gameObject);

            if (_shakeAngleZ > 0f)
            {
                _shakeSeq.Join(_rectTransform.DOShakeRotation(
                    _duration,
                    new Vector3(0f, 0f, _shakeAngleZ),
                    _shakeVibratoZ,
                    randomness: 25f,
                    fadeOut: true));
            }

            if (!_insideLayoutGroup && _shakePosX > 0f)
            {
                _shakeSeq.Join(_rectTransform.DOShakeAnchorPos(
                    _duration,
                    new Vector2(_shakePosX, 0f),
                    _shakeVibratoX,
                    randomness: 25f,
                    snapping: false,
                    fadeOut: true));
            }
        }
    }
}