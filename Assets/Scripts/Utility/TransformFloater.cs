using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility
{
    public class TransformFloater : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private float _distance = 0.15f;
        [SerializeField] private float _duration = 1.6f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Vector3 _initialLocalPosition;
        private Tween _floatTween;
        private bool _isInitialized;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDisable() => StopInstant();

        private void OnDestroy() => _floatTween?.Kill();

        public void Play()
        {
            EnsureInitialized();
            _floatTween?.Kill();

            transform.localPosition = _initialLocalPosition;

            var localUp = transform.parent != null
                ? transform.parent.InverseTransformDirection(Vector3.up)
                : Vector3.up;

            var targetLocalPos = _initialLocalPosition + localUp * _distance;

            _floatTween = transform
                .DOLocalMove(targetLocalPos, _duration)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopSmooth(float time = 0.15f)
        {
            _floatTween?.Kill();
            if (!_isInitialized) return;

            _floatTween = transform
                .DOLocalMove(_initialLocalPosition, time)
                .SetEase(Ease.OutQuad);
        }

        public void StopInstant()
        {
            _floatTween?.Kill();
            if (_isInitialized)
                transform.localPosition = _initialLocalPosition;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _initialLocalPosition = transform.localPosition;
            _isInitialized = true;
        }
    }
}