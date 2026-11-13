using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility
{
    public class TransformWobbler : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private float _angle = 4f;
        [SerializeField] private float _duration = 1.9f;
        [SerializeField] private Ease _ease = Ease.InOutSine;

        private Quaternion _initialLocalRotation;
        private Tween _wobbleTween;
        private bool _isInitialized;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDisable() => StopInstant();

        private void OnDestroy() => _wobbleTween?.Kill();

        public void Play()
        {
            EnsureInitialized();
            _wobbleTween?.Kill();

            transform.localRotation = _initialLocalRotation;

            var randomEuler = new Vector3(
                Random.Range(-_angle, _angle),
                Random.Range(-_angle * 0.5f, _angle * 0.5f),
                Random.Range(-_angle, _angle));

            var targetRot = _initialLocalRotation * Quaternion.Euler(randomEuler);

            _wobbleTween = transform
                .DOLocalRotateQuaternion(targetRot, _duration)
                .SetEase(_ease)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopSmooth(float time = 0.15f)
        {
            _wobbleTween?.Kill();
            if (!_isInitialized) return;

            _wobbleTween = transform
                .DOLocalRotateQuaternion(_initialLocalRotation, time)
                .SetEase(Ease.InOutSine);
        }

        public void StopInstant()
        {
            _wobbleTween?.Kill();
            if (_isInitialized)
                transform.localRotation = _initialLocalRotation;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _initialLocalRotation = transform.localRotation;
            _isInitialized = true;
        }
    }
}