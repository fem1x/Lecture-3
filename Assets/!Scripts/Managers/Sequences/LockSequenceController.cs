using System;
using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.UI;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class LockSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Slots")]
        [SerializeField] private CodeLockView _codeLockView;
        [SerializeField] private float _stampDelay = 0.15f;
        [SerializeField] private float _delayPerDice = 0.5f;
        
        [SerializeField] private float _validateDelayPerSlot = 0.2f;
        [SerializeField] private float _delayBeforeValidation = 0.8f;
        [SerializeField] private float _delayAfterValidation = 0.8f;
        
        [Header("Screen Flash")]
        [SerializeField] private float _flashAlpha = 0.5f;
        
        [Header("Camera Shake")]
        [SerializeField] private CameraShakePreset _successShake;
        [SerializeField] private CameraShakePreset _forcedShake;

        [Header("SFX")] 
        [SerializeField] private float _pitchStep = 0.08f;
        [SerializeField] private string _stampDiceSfx = "LockStampDice";
        [SerializeField] private string _stampSlotSfx = "LockStampSlot";
        [SerializeField] private string _slotCorrectSfx = "LockSlotCorrect";
        [SerializeField] private string _slotIncorrectSfx = "LockSlotIncorrect";
        [SerializeField] private string _successSfx = "LockSuccess";
        [SerializeField] private string _forcedSfx = "LockForced";
        
        
        private ISfxPlayer _sfxPlayer;
        private ScreenFlashView _screenFlashView;
        private ColorsConfig _colorsConfig;
        private CameraShaker _cameraShaker;

        [Inject]
        public void Construct(
            ISfxPlayer sfxPlayer, 
            ScreenFlashView screenFlashView, 
            ColorsConfig colorsConfig,
            CameraShaker cameraShaker)
        {
            _sfxPlayer = sfxPlayer;
            _screenFlashView = screenFlashView;
            _colorsConfig = colorsConfig;
            _cameraShaker = cameraShaker;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public async UniTask PlaySequenceAsync(List<Dice> dices, LockBreakResult result)
        {
            _codeLockView.ClearSlots();

            await InsertValuesAsync(dices);
            
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeValidation));
            await ValidationAsync(result);
            await UniTask.Delay(TimeSpan.FromSeconds(_delayAfterValidation));
            DoScreenFlash(result);
            DoCameraShake(result);
        }

        private async UniTask InsertValuesAsync(List<Dice> dices)
        {
            var slots = _codeLockView.Slots;
            for (int i = 0; i < dices.Count && i < _codeLockView.Slots.Count; i++)
            {
                var dice = dices[i];
                var slot = slots[i];
                
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerDice));
                
                dice.transform.DoScalePop().ToUniTask().Forget();
                PlayStampSfx(_stampDiceSfx, source: dice.transform, step: i);
                
                await UniTask.Delay(TimeSpan.FromSeconds(_stampDelay));
                
                slot.InsertValue(dice.Data.RolledValue);
                slot.DiceImage.transform.DoScalePop();
                PlayStampSfx(_stampSlotSfx);
            }
        }

        private async UniTask ValidationAsync(LockBreakResult result)
        {
            foreach (var slot in _codeLockView.Slots)
            {
                slot.SetCorrectState(result);
                PlayStampSfx(IsSuccess(result) ? _slotCorrectSfx : _slotIncorrectSfx);

                await UniTask.Delay(TimeSpan.FromSeconds(_validateDelayPerSlot));
            }
        }

        private void DoScreenFlash(LockBreakResult result)
        {
            var color = result == LockBreakResult.Success ? _colorsConfig.Success : _colorsConfig.Failure;
            _screenFlashView.DoFlash(color: color, alpha: _flashAlpha);
        }

        private void DoCameraShake(LockBreakResult result)
        {
            var preset = result == LockBreakResult.Success ? _successShake : _forcedShake;
            _cameraShaker.DoShake(preset);
        }
        
        private void PlayStampSfx(string sfxName, Transform source=null, int step=0)
        {
            var pitchMult = 1f + (_pitchStep * step);
            if (source != null)
                _sfxPlayer?.Play(sfxName, source.position, pitchMult);
            else
                _sfxPlayer?.Play(sfxName, pitchMult);
        }

        private bool IsSuccess(LockBreakResult result) => result ==  LockBreakResult.Success;
    }
}