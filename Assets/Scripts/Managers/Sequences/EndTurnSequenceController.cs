using System.Collections.Generic;
using System.Threading;
using _Scripts.Configs;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class EndTurnSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Timings")]
        [SerializeField] private float _delayPerDice = 0.12f;
        [SerializeField] private float _returnDelay = 0.14f;

        [Header("FX")]
        [SerializeField] private CameraShakePreset _damageShake;
        [SerializeField] private CameraShakePreset _degradeShake;
        [SerializeField] private CameraShakePreset _blockedShake;
        [Space(0.5f)]
        [SerializeField] private string _damageSfx = "DiceDamage";
        [SerializeField] private string _degradeSfx = "DiceDegrade";
        [SerializeField] private string _blockedSfx = "DiceBlocked";

        #region DI
        private DiceDamageService _damageService;
        private ReturnSequenceController _returnSequenceController;
        private GameSpeedConfig _speedConfig;
        private ISfxPlayer _sfxPlayer;
        private CameraShaker _cameraShaker;

        [Inject]
        public void Construct(
            DiceDamageService damageService,
            ReturnSequenceController returnSequenceController,
            GameSpeedConfig speedConfig,
            ISfxPlayer sfxPlayer,
            CameraShaker cameraShaker)
        {
            _damageService = damageService;
            _returnSequenceController = returnSequenceController;
            _speedConfig = speedConfig;
            _sfxPlayer = sfxPlayer;
            _cameraShaker = cameraShaker;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public async UniTask ProcessAllDicesAsync(IReadOnlyList<Dice> dices, bool forceDegrade, CancellationToken token = default)
        {
            if (dices == null || dices.Count == 0) return;

            var tasks = new List<UniTask>(dices.Count);

            for (int i = 0; i < dices.Count; i++)
            {
                var dice = dices[i];
                var step = forceDegrade
                    ? _damageService.DegradeDice(dice, restoreHp: false)
                    : _damageService.DamageDice(dice, dices);

                tasks.Add(ProcessSingleDiceAsync(step, i, token));

                if (i < dices.Count - 1)
                    await _speedConfig.DelayAsync(_delayPerDice, token);
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask ProcessSingleDiceAsync(DamageStep step, int index, CancellationToken token)
        {
            if (step.Outcome != DamageOutcome.None)
            {
                PlayDamageFx(step);
                await _speedConfig.DelayAsync(_returnDelay, token);
            }

            await _returnSequenceController.ReturnDiceAsync(step.TargetDice, index, token);
        }

        private void PlayDamageFx(DamageStep step)
        {
            var dice = step.TargetDice;
            var diceTransform = dice.transform;
            diceTransform.DoScalePop(scaleMult: 1.4f);
            
            switch (step.Outcome)
            {
                case DamageOutcome.Damaged:
                    dice.Data.NotifyDataChanged();
                    _cameraShaker.DoShake(_damageShake);
                    _sfxPlayer.Play(_damageSfx, diceTransform.position);
                    break;

                case DamageOutcome.Degraded:
                    dice.Data.NotifyDataChanged();
                    _cameraShaker.DoShake(_degradeShake);
                    _sfxPlayer.Play(_degradeSfx, diceTransform.position);
                    break;

                case DamageOutcome.Blocked:
                    _cameraShaker.DoShake(_blockedShake);
                    _sfxPlayer.Play(_blockedSfx, diceTransform.position);
                    break;
            }
        }
    }
}