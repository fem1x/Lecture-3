using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class VictoryShowcaseAnimationView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Containers")]
        [SerializeField] private Transform _podiumTransform;
        [SerializeField] private Transform _diceContainerTransform;

        [Header("Podium Settings")]
        [SerializeField] private float _podiumDuration = 0.5f;
        [SerializeField] private Ease _podiumEase = Ease.OutBack;

        [Header("Dice Drop Settings")]
        [SerializeField] private float _diceDropHeight = 6f;
        [SerializeField] private float _diceDropDuration = 0.65f;
        [SerializeField] private Ease _diceDropEase = Ease.OutBounce;
        [SerializeField] private Vector3 _diceDropSpinEuler = new(360f, 180f, 0f);

        [Header("Dice Appearance Scale")]
        [SerializeField] private float _diceScaleDuration = 0.25f;
        [SerializeField] private Ease _diceScaleEase = Ease.OutBack;

        [Header("Impact Settings")]
        [SerializeField] private float _impactForce = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _secondImpactRatio = 0.4f;
        [SerializeField] private float _impactDuration = 0.2f;
        [SerializeField] private Vector3 _punchAxisMask = new(0.8f, -1f, 0.8f);

        private Vector3 _podiumInitialScale;
        private Vector3 _diceInitialLocalPosition;
        private Vector3 _diceInitialLocalScale;
        private Quaternion _diceInitialLocalRotation;
        private Sequence _showcaseSequence;
        private bool _isInitialized;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDestroy() => _showcaseSequence?.Kill();

        public async UniTask PlayShowAsync(CancellationToken token)
        {
            EnsureInitialized();
            _showcaseSequence?.Kill();

            PrepareInitialStates();
            gameObject.SetActive(true);

            _showcaseSequence = DOTween.Sequence();

            // 1. Подиум пружинит снизу вверх
            _showcaseSequence.Append(_podiumTransform
                .DOScale(_podiumInitialScale, _podiumDuration)
                .SetEase(_podiumEase));

            // 2. Кубик падает с легким нахлестом
            var diceDropStartTime = _podiumDuration * 0.35f;

            // Включаем кубик и даем ему скейл-ин строго в момент старта падения
            _showcaseSequence.InsertCallback(diceDropStartTime, () => _diceContainerTransform.gameObject.SetActive(true));
            _showcaseSequence.Insert(diceDropStartTime, _diceContainerTransform
                .DOScale(_diceInitialLocalScale, _diceScaleDuration)
                .SetEase(_diceScaleEase));

            _showcaseSequence.Insert(diceDropStartTime, _diceContainerTransform
                .DOLocalMoveY(_diceInitialLocalPosition.y, _diceDropDuration)
                .SetEase(_diceDropEase));

            _showcaseSequence.Insert(diceDropStartTime, _diceContainerTransform
                .DOLocalRotate(_diceInitialLocalRotation.eulerAngles, _diceDropDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutCubic));

            // 3. Первый сильный удар
            var firstImpactVector = _punchAxisMask * _impactForce;
            var firstImpactTime = diceDropStartTime + (_diceDropDuration * 0.364f);
            
            _showcaseSequence.Insert(firstImpactTime, _podiumTransform
                .DOPunchScale(firstImpactVector, _impactDuration, vibrato: 5, elasticity: 0.5f));

            // 4. Второй слабый отскок
            var secondImpactVector = firstImpactVector * _secondImpactRatio;
            var secondImpactTime = diceDropStartTime + (_diceDropDuration * 0.727f);
            
            _showcaseSequence.Insert(secondImpactTime, _podiumTransform
                .DOPunchScale(secondImpactVector, _impactDuration * 0.75f, vibrato: 4, elasticity: 0.4f));

            await _showcaseSequence.WithCancellation(token);
        }

        public void HideInstant()
        {
            EnsureInitialized();
            _showcaseSequence?.Kill();

            _podiumTransform.localScale = Vector3.zero;
            _diceContainerTransform.localScale = Vector3.zero;
            _diceContainerTransform.localPosition = _diceInitialLocalPosition + (Vector3.up * _diceDropHeight);
            _diceContainerTransform.gameObject.SetActive(false);

            gameObject.SetActive(false);
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            _podiumInitialScale = _podiumTransform.localScale;
            _diceInitialLocalPosition = _diceContainerTransform.localPosition;
            _diceInitialLocalScale = _diceContainerTransform.localScale;
            _diceInitialLocalRotation = _diceContainerTransform.localRotation;

            _isInitialized = true;
        }

        private void PrepareInitialStates()
        {
            _podiumTransform.localScale = Vector3.zero;

            _diceContainerTransform.localScale = Vector3.zero;
            _diceContainerTransform.localPosition = _diceInitialLocalPosition + (Vector3.up * _diceDropHeight);
            _diceContainerTransform.localEulerAngles = _diceInitialLocalRotation.eulerAngles + _diceDropSpinEuler;
            _diceContainerTransform.gameObject.SetActive(false);
        }
    }
}