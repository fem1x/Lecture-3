using System;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    [RequireComponent(typeof(Collider))]
    public class VictoryDiceRollView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int> RollCompleted;

        [Header("Jump Settings")]
        [SerializeField] private float _jumpHeight = 1.2f;
        [SerializeField] private float _duration = 0.85f;
        [SerializeField, Range(0.1f, 0.9f)] private float _jumpUpTimeShare = 0.45f;
        [SerializeField] private Ease _jumpUpEase = Ease.OutQuad;
        [SerializeField] private Ease _fallDownEase = Ease.InQuad;

        [Header("Rotation Settings")]
        [SerializeField] private int _minFullSpins = 2;
        [SerializeField] private int _maxFullSpins = 3;
        [SerializeField] private Ease _rotationEase = Ease.OutCubic;
        [SerializeField] private bool _randomizeSpinDirection = true;

        [Header("Squash / Impact Deformation")]
        [SerializeField, Range(0f, 0.8f)] private float _verticalSquashAmount = 0.3f;
        [SerializeField, Range(0f, 0.8f)] private float _horizontalExpandAmount = 0.18f;

        [Header("Squash Timing & Curves")]
        [SerializeField] private float _compressDuration = 0.05f;
        [SerializeField] private Ease _compressEase = Ease.OutQuad;

        [SerializeField] private float _reboundDuration = 0.22f;
        [SerializeField] private Ease _reboundEase = Ease.OutBack;

        [Header("Face Rotations (Target to Camera)")]
        [SerializeField] private Vector3[] _faceEulerAngles =
        {
            new(0f, 0f, 0f),
            new(0f, 90f, 0f),
            new(-90f, 0f, 0f),
            new(90f, 0f, 0f),
            new(0f, -90f, 0f),
            new(180f, 0f, 0f)
        };

        private Vector3 _initialLocalPosition;
        private Vector3 _initialLocalScale;
        private bool _isRolling;
        private Sequence _rollSequence;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _initialLocalPosition = transform.localPosition;
            _initialLocalScale = transform.localScale;
        }

        private void OnDestroy() => _rollSequence?.Kill();

        private void OnMouseDown()
        {
            if (_isRolling) return;

            var randomValue = UnityEngine.Random.Range(1, 7);
            RollToValue(randomValue);
        }

        public void RollToValue(int targetValue)
        {
            _isRolling = true;
            _rollSequence?.Kill();

            transform.localPosition = _initialLocalPosition;
            transform.localScale = _initialLocalScale;

            var targetFaceRotation = _faceEulerAngles[targetValue - 1];

            var spinsCount = UnityEngine.Random.Range(_minFullSpins, _maxFullSpins + 1);
            var spinMultiplier = 360f * spinsCount;

            var dirX = _randomizeSpinDirection && UnityEngine.Random.value > 0.5f ? -1f : 1f;
            var dirY = _randomizeSpinDirection && UnityEngine.Random.value > 0.5f ? -1f : 1f;

            var totalSpinEuler = targetFaceRotation + new Vector3(spinMultiplier * dirX, spinMultiplier * dirY, 0f);

            var targetRot = Quaternion.Euler(targetFaceRotation);
            var invRot = Quaternion.Inverse(targetRot);
            var localUp = invRot * Vector3.up;

            var squashMultiplier = new Vector3(
                Mathf.Abs(localUp.x) > 0.5f ? -_verticalSquashAmount : _horizontalExpandAmount,
                Mathf.Abs(localUp.y) > 0.5f ? -_verticalSquashAmount : _horizontalExpandAmount,
                Mathf.Abs(localUp.z) > 0.5f ? -_verticalSquashAmount : _horizontalExpandAmount
            );

            var targetSquashedScale = _initialLocalScale + Vector3.Scale(_initialLocalScale, squashMultiplier);

            var upTime = _duration * _jumpUpTimeShare;
            var downTime = _duration * (1f - _jumpUpTimeShare);

            _rollSequence = DOTween.Sequence();

            // 1. Вращение
            _rollSequence.Insert(0f, transform
                .DOLocalRotate(totalSpinEuler, _duration, RotateMode.FastBeyond360)
                .SetEase(_rotationEase));

            // 2. Взлет
            _rollSequence.Insert(0f, transform
                .DOLocalMoveY(_initialLocalPosition.y + _jumpHeight, upTime)
                .SetEase(_jumpUpEase));

            // 3. Падение на плоскость
            _rollSequence.Insert(upTime, transform
                .DOLocalMoveY(_initialLocalPosition.y, downTime)
                .SetEase(_fallDownEase));

            // 4. Мгновенное сжатие в пол (0.04-0.06s)
            _rollSequence.Insert(_duration, transform
                .DOScale(targetSquashedScale, _compressDuration)
                .SetEase(_compressEase));

            // 5. Затяжной пружинящий возврат в исходную форму
            _rollSequence.Insert(_duration + _compressDuration, transform
                .DOScale(_initialLocalScale, _reboundDuration)
                .SetEase(_reboundEase));

            _rollSequence
                .OnComplete(() =>
                {
                    _isRolling = false;
                    RollCompleted?.Invoke(targetValue);
                })
                .SetLink(gameObject);
        }

        public void ResetView()
        {
            _rollSequence?.Kill();
            _isRolling = false;
            transform.localPosition = _initialLocalPosition;
            transform.localScale = _initialLocalScale;
        }
    }
}