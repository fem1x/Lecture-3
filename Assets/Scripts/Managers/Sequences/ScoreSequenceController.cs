using System;
using System.Threading;
using _Scripts.Configs;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using _Scripts.Utility;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class ScoreSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Texts")]
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _multText;
        
        [Header("Timings")]
        [SerializeField] private float _delayPerStep = 0.22f;
        [SerializeField] private float _delayBeforeFinalMult = 0.25f;
        [SerializeField] private float _delayAfterFinalMult = 0.35f;
        [Space(0.5f)]
        [SerializeField] private float _pointsMultDuration = 0.5f;
        [SerializeField] private float _pointsAddDuration = 0.15f;

        [Header("Scale Pop")] 
        [SerializeField] private float _dicePopMult = 1.6f;
        [SerializeField] private float _uiPopMult = 1.88f;
        
        [Header("Camera shake")]
        [SerializeField] private CameraShakePreset _diceScoreShake;
        [SerializeField] private CameraShakePreset _multTriggerShake;
        
        [Header("SFX")]
        [SerializeField] private string _addPointsSfx = "AddPoints";
        [SerializeField] private string _triggerMultSfx = "TriggerMult";
        [SerializeField] private string _addScoreSfx = "AddScore";
        [SerializeField] private string _charmTriggerSfx = "CharmTrigger";
        
        private int _currentPoints;
        private int _currentMult;
        private int _fxStepIndex;
        
        #region DI
        private ISfxPlayer _sfxPlayer;
        private ScoreManager _scoreManager;
        private CameraShaker _cameraShaker;
        private GameStateManager _gameStateManager;
        private GameSpeedConfig _speedConfig;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(
            ISfxPlayer sfxPlayer, 
            ScoreManager scoreManager, 
            CameraShaker cameraShaker, 
            GameStateManager gameStateManager,
            GameSpeedConfig speedConfig)
        {
            _sfxPlayer = sfxPlayer;
            _scoreManager = scoreManager;
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

        public async UniTask PlaySequenceAsync(ScoreSequencePlan sequencePlan)
        {
            CancelSequence();
            _cts = new();
            var token = _cts.Token;

            ResetState(sequencePlan);

            try
            {
                foreach (var step in sequencePlan.Steps)
                {
                    switch (step.Type)
                    {
                        case ScoreStepType.DiceScored:
                            await DiceStepAsync(step, token);
                            break;
                        
                        case ScoreStepType.CharmTrigger:
                            await CharmStepAsync(step, token);
                            break;
                        
                        case ScoreStepType.CombinationBase:
                            await CombinationStepAsync(step, token);
                            break;
                        
                        case ScoreStepType.Multiply:
                            await MultiplyStepAsync(step, token);
                            break;
                    }
                }
                
                await _speedConfig.DelayAsync(_delayAfterFinalMult, token);
                ApplyFinalScore(sequencePlan.TotalScore);
            }
            catch (OperationCanceledException)
            {
                ResetTextScales();
            }
        }
        
        #region Step async Methods
        private async UniTask DiceStepAsync(ScoreStep step, CancellationToken token)
        {
            await _speedConfig.DelayAsync(_delayPerStep, token);
            PlayDiceStepFx(step.SourceTransform, _addPointsSfx, true);
            await AddPointsAsync(step.BonusPoints, token);
        }

        private async UniTask CharmStepAsync(ScoreStep step, CancellationToken token)
        {
            await _speedConfig.DelayAsync(_delayPerStep, token);
    
            step.CharmView.PlayTriggerAnimation();
            _sfxPlayer?.Play(_charmTriggerSfx);

            // +Points
            if (step.BonusPoints > 0)
            {
                _pointsText.transform.DoScalePop(_uiPopMult);
                await AddPointsAsync(step.BonusPoints, token);
            }

            // +Mult
            if (step.BonusMultiplier > 0)
            {
                _multText.transform.DoScalePop(_uiPopMult);
                await AddMultAsync(step.BonusMultiplier, token);
            }

            // X-Mult
            if (step.XMultiplier > 1f)
            {
                _multText.transform.DoScalePop(2.5f);
                await XMultAsync(step.XMultiplier, token);
            }
        }

        private async UniTask CombinationStepAsync(ScoreStep step, CancellationToken token)
        {
            if (step.BonusPoints == 0 && step.BonusMultiplier == 0) return;
            
            await _speedConfig.DelayAsync(_delayPerStep, token);
            PlayDiceStepFx(step.SourceTransform, _addPointsSfx, true);
            
            if (step.BonusPoints > 0)
                await AddPointsAsync(step.BonusPoints, token);

            if (step.BonusMultiplier > 0)
                await AddMultAsync(step.BonusMultiplier, token);
        }

        private async UniTask MultiplyStepAsync(ScoreStep step, CancellationToken token)
        {
            await _speedConfig.DelayAsync(_delayBeforeFinalMult, token);
            await TriggerMultAsync(token);
        }
        #endregion
        
        private async UniTask AddPointsAsync(int amount, CancellationToken token)
        {
            var target = _currentPoints + amount;
            await DoTextValueTween(_pointsText, _currentPoints, target, _speedConfig.ScaleDuration(_pointsAddDuration), token);
            _currentPoints = target;
        }

        private async UniTask AddMultAsync(int amount, CancellationToken token)
        {
            var target = _currentMult + amount;
            await DoTextValueTween(_multText, _currentMult, target, _speedConfig.ScaleDuration(_pointsAddDuration), token);
            _currentMult = target;
        }
        
        private async UniTask XMultAsync(float xMult, CancellationToken token)
        {
            var target = Mathf.RoundToInt(_currentMult * xMult);
            await DoTextValueTween(_multText, _currentMult, target, _speedConfig.ScaleDuration(_pointsAddDuration), token);
            _currentMult = target;
        }
        
        private void ApplyFinalScore(int totalScore)
        {
            _scoreManager.AddScore(totalScore);
            _sfxPlayer?.Play(_addScoreSfx);
        }
        
        private async UniTask TriggerMultAsync(CancellationToken token)
        {
            var scaledMultDuration = _speedConfig.ScaleDuration(_pointsMultDuration);
            PlayMultTriggerFx(scaledMultDuration);
            var targetPoints = _currentPoints * _currentMult;
            
            await UniTask.WhenAll(
                DoTextValueTween(_pointsText, _currentPoints, targetPoints, scaledMultDuration, token),
                DoTextValueTween(_multText, _currentMult, 1, scaledMultDuration, token));

            _currentPoints = targetPoints;
            _currentMult = 1;
        }
        
        private void ResetState(ScoreSequencePlan sequencePlan)
        {
            _currentPoints = sequencePlan.Combination.BasePoints;
            _currentMult = sequencePlan.Combination.Multiplier;
            _fxStepIndex = 0;

            ResetTextScales();
            _pointsText.text = _currentPoints.ToString();
            _multText.text = _currentMult.ToString();
        }

        private void ResetTextScales()
        {
            _pointsText.transform.DOKill();
            _multText.transform.DOKill();
            _pointsText.transform.localScale = Vector3.one;
            _multText.transform.localScale = Vector3.one;
        }
        
        #region FX Methods
        private void PlayDiceStepFx(Transform source, string sfxName, bool is3D)
        {
            source.DoScalePop(_dicePopMult);
            _pointsText.transform.DoScalePop(_uiPopMult);
            _cameraShaker.DoShake(_diceScoreShake);

            if (sfxName == null) return;
            
            var pitchMult = 1f + (0.09f * _fxStepIndex++);
            if (is3D)
                _sfxPlayer?.Play(sfxName, source.position, pitchMult);
            else
                _sfxPlayer?.Play(sfxName, pitchMult);
        }

        private void PlayMultTriggerFx(float duration)
        {
            _multText.transform.DoScalePop(3f, outTime: duration);
            _pointsText.transform.DoScalePop(3f, outTime: duration);
            _cameraShaker.DoShake(_multTriggerShake);
            _sfxPlayer?.Play(_triggerMultSfx);
        }
        #endregion

        #region Tweens
        private async UniTask DoTextValueTween(TMP_Text text, int fromValue, int toValue, float duration, CancellationToken token)
        {
            text.DOKill();
            var displayedValue = fromValue;
            await DOTween.To(
                    () => displayedValue,
                    x =>
                    {
                        displayedValue = x;
                        text.text = x.ToString();
                    },
                    toValue,
                    duration
                )
                .SetEase(Ease.OutQuad)
                .ToUniTask(cancellationToken: token);

            text.text = toValue.ToString();
        }
        #endregion
    }
}