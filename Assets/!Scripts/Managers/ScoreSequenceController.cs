using System;
using System.Collections.Generic;
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
        
        [Header("Camera & Shake")]
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
        
        private ISfxPlayer _sfxPlayer;
        private ScoreManager _scoreManager;
        private CameraShaker _cameraShaker;
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, ScoreManager  scoreManager, CameraShaker cameraShaker)
        {
            _sfxPlayer = sfxPlayer;
            _scoreManager = scoreManager;
            _cameraShaker = cameraShaker;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public async UniTask PlaySequenceAsync(ScoreSequencePlan sequencePlan)
        {
            ResetState(sequencePlan);

            foreach (var step in sequencePlan.Steps)
            {
                switch (step.Type)
                {
                    case ScoreStepType.DiceScored:
                        await DiceStepAsync(step);
                        break;
                    
                    case ScoreStepType.CharmTrigger:
                        await CharmStepAsync(step);
                        break;
                    
                    case ScoreStepType.CombinationBase:
                        await CombinationStepAsync(step);
                        break;
                    
                    case ScoreStepType.Multiply:
                        await MultiplyStepAsync(step);
                        break;
                }
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_delayAfterFinalMult));
            ApplyFinalScore(sequencePlan.TotalScore);
        }
        
        #region Step async Methods
        private async UniTask DiceStepAsync(ScoreStep step)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delayPerStep));
            PlayDiceStepFx(step.SourceTransform, _addPointsSfx, true);
            await AddPointsAsync(step.BonusPoints);
        }

        private async UniTask CharmStepAsync(ScoreStep step)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delayPerStep));
    
            step.CharmView.PlayTriggerAnimation();
            _sfxPlayer?.Play(_charmTriggerSfx);

            // +Points
            if (step.BonusPoints > 0)
            {
                _pointsText.transform.DoScalePop(scaleMult: 1.8f);
                await AddPointsAsync(step.BonusPoints);
            }

            // +Mult
            if (step.BonusMultiplier > 0)
            {
                _multText.transform.DoScalePop(scaleMult: 1.8f);
                await AddMultAsync(step.BonusMultiplier);
            }

            // X-Mult
            if (step.XMultiplier > 1f)
            {
                _multText.transform.DoScalePop(scaleMult: 2.5f);
                await XMultAsync(step.XMultiplier);
            }
        }

        private async UniTask CombinationStepAsync(ScoreStep step)
        {
            if (step.BonusPoints == 0 && step.BonusMultiplier == 0) return;
            
            await UniTask.Delay(TimeSpan.FromSeconds(_delayPerStep));
            PlayDiceStepFx(step.SourceTransform, _addPointsSfx, true);
            
            if (step.BonusPoints > 0)
                await AddPointsAsync(step.BonusPoints);

            if (step.BonusMultiplier > 0)
                await AddMultAsync(step.BonusMultiplier);
        }

        private async UniTask MultiplyStepAsync(ScoreStep step)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeFinalMult));
            await TriggerMultAsync();
        }
        #endregion
        
        private async UniTask AddPointsAsync(int amount)
        {
            var target = _currentPoints + amount;
            await DoTextValueTween(_pointsText, _currentPoints, target, _pointsAddDuration);
            _currentPoints = target;
        }

        private async UniTask AddMultAsync(int amount)
        {
            var target = _currentMult + amount;
            await DoTextValueTween(_multText, _currentMult, target, _pointsAddDuration);
            _currentMult = target;
        }
        
        private async UniTask XMultAsync(float xMult)
        {
            var target = Mathf.RoundToInt(_currentMult * xMult);
            await DoTextValueTween(_multText, _currentMult, target, _pointsAddDuration);
            _currentMult = target;
        }
        
        private void ApplyFinalScore(int totalScore)
        {
            _scoreManager.AddScore(totalScore);
            _sfxPlayer?.Play(_addScoreSfx);
        }
        
        private async UniTask TriggerMultAsync()
        {
            PlayMultTriggerFx();
            var targetPoints = _currentPoints * _currentMult;
            
            await UniTask.WhenAll(
                DoTextValueTween(_pointsText, _currentPoints, targetPoints, _pointsMultDuration),
                DoTextValueTween(_multText, _currentMult, 1, _pointsMultDuration));

            _currentPoints = targetPoints;
            _currentMult = 1;
        }
        
        private void ResetState(ScoreSequencePlan sequencePlan)
        {
            _currentPoints = sequencePlan.Combination.BasePoints;
            _currentMult = sequencePlan.Combination.Multiplier;
            _fxStepIndex = 0;

            _pointsText.text = _currentPoints.ToString();
            _multText.text = _currentMult.ToString();
        }
        
        #region FX Methods

        private void PlayDiceStepFx(Transform source, string sfxName, bool is3D)
        {
            source.DoScalePop();
            _pointsText.transform.DoScalePop(scaleMult: 1.8f);
            _cameraShaker.Shake(_diceScoreShake);

            //SFX
            if (sfxName == null) return;
            
            var pitchMult = 1f + (0.09f * _fxStepIndex++);
            if (is3D)
                _sfxPlayer?.Play(sfxName, source.position, pitchMult);
            
            else
                _sfxPlayer?.Play(sfxName, pitchMult);
        }

        private void PlayMultTriggerFx()
        {
            _multText.transform.DoScalePop(3f, outTime: _pointsMultDuration);
            _pointsText.transform.DoScalePop(3f, outTime: _pointsMultDuration);
            _cameraShaker.Shake(_multTriggerShake);
            _sfxPlayer?.Play(_triggerMultSfx);
        }
        #endregion

        #region Tweens
        private async UniTask DoTextValueTween(TMP_Text text, int fromValue, int toValue, float duration)
        {
            text.DOKill(true);
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
                .ToUniTask();

            text.text = toValue.ToString();
        }
        #endregion
    }
}