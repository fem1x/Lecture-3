using System;
using System.Threading;
using _Scripts.Configs;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class DamageSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Timings")]
        [SerializeField] private float _delayPerDice = 0.2f;
        [SerializeField] private float _delayAfterSequence = 0.35f;
        [Space(0.5f)]
        [SerializeField] private float _popInTime = 0.07f;
        [SerializeField] private float _popOutTime = 0.25f;

        [Header("Scale Pop")] 
        [SerializeField] private float _damagePopMult = 1.3f;
        [SerializeField] private float _degradePopMult = 1.5f;
        
        [Header("FX")]
        [SerializeField] private CameraShakePreset _damageShake;
        [SerializeField] private CameraShakePreset _degradeShake;
        [SerializeField] private string _damageSfx = "DiceDamage";
        [SerializeField] private string _degradeSfx = "DiceDegrade";

        #region DI
        private ISfxPlayer _sfxPlayer;
        private CameraShaker _cameraShaker;
        private GameSpeedConfig _speedConfig;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, CameraShaker cameraShaker, GameSpeedConfig speedConfig)
        {
            _sfxPlayer = sfxPlayer;
            _cameraShaker = cameraShaker;
            _speedConfig = speedConfig;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public async UniTask PlaySequenceAsync(DamageSequencePlan plan, CancellationToken token = default)
        {
            if (plan == null || plan.Steps.Count == 0) return;

            try
            {
                foreach (var step in plan.Steps)
                {
                    await _speedConfig.DelayAsync(_delayPerDice, token);
                    
                    step.TargetDice.Data.NotifyDataChanged();
                    PlayStepFx(step);
                }

                await _speedConfig.DelayAsync(_delayAfterSequence, token);
            }
            catch (OperationCanceledException)
            {
                foreach (var step in plan.Steps)
                    step.TargetDice.Data.NotifyDataChanged();
            }
        }

        private void PlayStepFx(DamageStep step)
        {
            var diceTransform = step.TargetDice.transform;
            
            var popMult = step.IsDegraded ? _degradePopMult : _damagePopMult;
            diceTransform.DoScalePop(scaleMult: popMult, _popInTime, _popOutTime);

            _cameraShaker.DoShake(step.IsDegraded ? _degradeShake : _damageShake);
            _sfxPlayer.Play(step.IsDegraded ? _degradeSfx : _damageSfx, diceTransform.position);
        }
    }
}