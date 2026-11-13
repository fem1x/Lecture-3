using _Scripts.Utility;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Dices
{
    public class DiceDamageFeedback : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Hit Flash")] 
        [ColorUsage(true, true)] 
        [SerializeField] private Color _damageFlashColor;
        [ColorUsage(true, true)] 
        [SerializeField] private Color _degradeFlashColor;
        [ColorUsage(true, true)] 
        [SerializeField] private Color _blockedFlashColor; 
        [SerializeField] private float _flashDuration = 0.18f;

        [Header("Hit Punch")]
        [SerializeField] private float _damagePunchAngle = 16f;
        [SerializeField] private float _degradePunchAngle = 24f;
        [SerializeField] private float _blockedPunchOffset = 0.08f;
        [SerializeField] private float _punchTime = 0.22f;

        [Header("References")]
        [SerializeField] private HitFlasher _hitFlasher;
        [SerializeField] private Transform _visualRoot;

        private Vector3 _initialLocalPos;
        private Quaternion _initialLocalRot;
        private bool _isInitialized;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => EnsureInitialized();
        private void OnDisable() => ResetImmediate();
        private void OnDestroy() => _visualRoot.DOKill();

        public void PlayDamageHit()
        {
            _hitFlasher.Flash(_damageFlashColor, _flashDuration);
            _visualRoot.DoRotationPop(_damagePunchAngle, _punchTime);
        }

        public void PlayDegradeHit()
        {
            _hitFlasher.Flash(_degradeFlashColor, _flashDuration);
            _visualRoot.DoRotationPop(_degradePunchAngle, _punchTime);
        }

        public void PlayBlockedHit()
        {
            _hitFlasher.Flash(_blockedFlashColor, _flashDuration);

            var randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)).normalized * _blockedPunchOffset;

            _visualRoot.DOPunchPosition(randomDir, _punchTime, vibrato: 10, elasticity: 0.4f);
        }

        public void ResetImmediate()
        {
            _hitFlasher.StopImmediate();
            _visualRoot.DOKill();

            if (!_isInitialized) return;
            _visualRoot.localPosition = _initialLocalPos;
            _visualRoot.localRotation = _initialLocalRot;
        }
        
        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _initialLocalPos = _visualRoot.localPosition;
            _initialLocalRot = _visualRoot.localRotation;
            _isInitialized = true;
        }
    }
}