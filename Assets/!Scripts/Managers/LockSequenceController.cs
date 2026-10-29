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
        
        private ISfxPlayer _sfxPlayer;
        private ScreenFlashView _screenFlashView;
        private ColorsConfig _colorsConfig;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, ScreenFlashView screenFlashView, ColorsConfig colorsConfig)
        {
            _sfxPlayer = sfxPlayer;
            _screenFlashView = screenFlashView;
            _colorsConfig = colorsConfig;
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
        }

        private async UniTask InsertValuesAsync(List<Dice> dices)
        {
            var slots = _codeLockView.Slots;
            for (int i = 0; i < dices.Count && i < _codeLockView.Slots.Count; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerDice));
                
                dices[i].transform.DoScalePop().ToUniTask().Forget();
                
                await UniTask.Delay(TimeSpan.FromSeconds(_stampDelay));
                
                slots[i].InsertValue(dices[i].Data.RolledValue);
            }
        }

        private async UniTask ValidationAsync(LockBreakResult result)
        {
            foreach (var slot in _codeLockView.Slots)
            {
                slot.SetCorrectState(result);
                await UniTask.Delay(TimeSpan.FromSeconds(_validateDelayPerSlot));
            }
        }

        private void DoScreenFlash(LockBreakResult result)
        {
            var color = result == LockBreakResult.Success ? _colorsConfig.Success : _colorsConfig.Failure;
            _screenFlashView.DoFlash(color: color, alpha: _flashAlpha);
        }
    }
}