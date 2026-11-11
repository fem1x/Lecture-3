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

        private void OnDisable() => Stop(resetPosition: true);

        private void OnDestroy() => _floatTween?.Kill();

        public void Play()
        {
            EnsureInitialized();
            _floatTween?.Kill();

            transform.localPosition = _initialLocalPosition;

            _floatTween = transform
                .DOLocalMoveY(_initialLocalPosition.y + _distance, _duration)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void Stop(bool resetPosition = false)
        {
            _floatTween?.Kill();

            if (resetPosition && _isInitialized)
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