using System.Collections.Generic;
using System.Threading;
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
        [Header("Lock View")]
        [SerializeField] private CodeLockView _codeLockView;
        
        [Header("Timings")]
        [SerializeField] private float _stampDelay = 0.15f;
        [SerializeField] private float _delayPerDice = 0.5f;
        [Space]
        [SerializeField] private float _validateDelayPerSlot = 0.2f;
        [SerializeField] private float _delayBeforeValidation = 0.8f;
        [SerializeField] private float _delayAfterValidation = 0.8f;
        [SerializeField] private float _delayAfterUnlock = 0.5f;
        
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
        
        private CancellationTokenSource _cts;
        
        #region DI
        private ISfxPlayer _sfxPlayer;
        private ScreenFlashView _screenFlashView;
        private ColorsConfig _colorsConfig;
        private CameraShaker _cameraShaker;
        private GameStateManager _gameStateManager;
        private GameSpeedConfig _speedConfig;

        [Inject]
        public void Construct(
            ISfxPlayer sfxPlayer, 
            ScreenFlashView screenFlashView, 
            ColorsConfig colorsConfig,
            CameraShaker cameraShaker,
            GameStateManager gameStateManager,
            GameSpeedConfig speedConfig)
        {
            _sfxPlayer = sfxPlayer;
            _screenFlashView = screenFlashView;
            _colorsConfig = colorsConfig;
            _cameraShaker = cameraShaker;
            _gameStateManager = gameStateManager;
            _speedConfig = speedConfig;
            
            _gameStateManager.StateChanged += OnStateChanged;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;
                
            CancelSequence();
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Game)
                CancelSequence();
        }
        
        private void CancelSequence()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public async UniTask PlaySequenceAsync(List<Dice> dices, LockBreakResult result)
        {
            CancelSequence();
            _cts = new();
            var token = _cts.Token;

            _codeLockView.ClearSlots();

            var isCancelled = await InsertValuesAsync(dices, token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            isCancelled = await _speedConfig.DelayAsync(_delayBeforeValidation, token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            isCancelled = await ValidationAsync(result, token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            isCancelled = await _speedConfig.DelayAsync(_delayAfterValidation, token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            DoResultFx(result);
            
            await _speedConfig.DelayAsync(_delayAfterUnlock, token).SuppressCancellationThrow();
        }

        private async UniTask InsertValuesAsync(List<Dice> dices, CancellationToken token)
        {
            var slots = _codeLockView.Slots;
            for (int i = 0; i < dices.Count && i < _codeLockView.Slots.Count; i++)
            {
                var dice = dices[i];
                var slot = slots[i];
                
                await _speedConfig.DelayAsync(_delayPerDice, token);
                
                dice.transform.DoScalePop();
                PlayStampSfx(_stampDiceSfx, source: dice.transform, step: i);
                
                await _speedConfig.DelayAsync(_stampDelay, token);
                
                slot.InsertValue(dice.Data.RolledValue);
                slot.DiceImage.transform.DoScalePop();
                PlayStampSfx(_stampSlotSfx);
            }
        }

        private async UniTask ValidationAsync(LockBreakResult result, CancellationToken token)
        {
            foreach (var slot in _codeLockView.Slots)
            {
                slot.SetCorrectState(result);
                PlayStampSfx(IsSuccess(result) ? _slotCorrectSfx : _slotIncorrectSfx);

                await _speedConfig.DelayAsync(_validateDelayPerSlot, token);
            }
        }

        private void DoScreenFlash(LockBreakResult result)
        {
            var color = IsSuccess(result) ? _colorsConfig.Success : _colorsConfig.Failure;
            _screenFlashView.DoFlash(color: color, alpha: _flashAlpha);
        }

        private void DoCameraShake(LockBreakResult result)
        {
            var preset = IsSuccess(result) ? _successShake : _forcedShake;
            _cameraShaker.DoShake(preset);
        }
        
        private void PlayStampSfx(string sfxName, Transform source = null, int step = 0)
        {
            var pitchMult = 1f + (_pitchStep * step);
            if (source != null)
                _sfxPlayer?.Play(sfxName, source.position, pitchMult);
            else
                _sfxPlayer?.Play(sfxName, pitchMult);
        }

        private void DoResultFx(LockBreakResult result)
        {
            var sfxName = IsSuccess(result) ? _successSfx : _forcedSfx;
            
            _sfxPlayer.Play(sfxName);
            DoScreenFlash(result);
            DoCameraShake(result);
        }

        private bool IsSuccess(LockBreakResult result) => result == LockBreakResult.Success;
    }
}