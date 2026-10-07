using System;
using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.Utility;
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
        [SerializeField] private float _delayPerDice = 0.22f;
        [SerializeField] private float _delayBeforeMult = 0.25f;
        [SerializeField] private float _delayAfterMult = 0.35f;
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
        
        private Vector3 _originalCamPos;
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
        
        public async UniTask PlaySequenceAsync(List<Dice> dices, ScoreCalculationResult scoreResult)
        {
            var currentPoints = await AddPointsAsync(dices, scoreResult.Combination.BasePoints);
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeMult));
            await TriggerMultAsync(currentPoints, scoreResult.Combination.Multiplier);
            await UniTask.Delay(TimeSpan.FromSeconds(_delayAfterMult));
            ApplyFinalScore(scoreResult.TotalScore);
        }
        
        private async UniTask<int> AddPointsAsync(List<Dice> dices, int basePoints)
        {
            var currentPoints = basePoints;
            var scoredDiceCount = 0;
            
            for (int i = 0; i < dices.Count; i++)
            {
                var dice = dices[i];
                if (dice.Data.RolledValue <= 0) continue;
                
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerDice));
                
                var pitchMult =  1f + (0.11f * scoredDiceCount);
                PlayPointsAddFx(dice, pitchMult);
                scoredDiceCount++;
                
                var fromPoints = currentPoints;
                currentPoints += dice.Data.RolledValue;
                
                await DoTextValueTween(_pointsText, fromPoints, currentPoints, _pointsAddDuration);
            }
            return currentPoints;
        }

        private async UniTask TriggerMultAsync(int currentPoints, int multiplier)
        {
            PlayMultTriggerFx();
            var toPoints = currentPoints * multiplier;
            
            await UniTask.WhenAll(
                DoTextValueTween(_pointsText, currentPoints, toPoints, _pointsMultDuration),
                DoTextValueTween(_multText, multiplier, 1, _pointsMultDuration));
        }
        
        private void PlayPointsAddFx(Dice dice, float pitchMult)
        {
            DoScalePopTween(dice.transform);
            DoScalePopTween(_pointsText.transform, 1.8f);
            _cameraShaker.Shake(_diceScoreShake);
            _sfxPlayer?.Play(_addPointsSfx, dice.transform.position, pitchMult);
        }

        private void PlayMultTriggerFx()
        {
            DoScalePopTween(_multText.transform, 3f, outTime: _pointsMultDuration);
            DoScalePopTween(_pointsText.transform, 3f, outTime: _pointsMultDuration);
            _cameraShaker.Shake(_multTriggerShake);
            _sfxPlayer?.Play(_triggerMultSfx);
        }
                
        private void ApplyFinalScore(int totalScore)
        {
            _scoreManager.AddScore(totalScore);
            _sfxPlayer?.Play(_addScoreSfx);
        }

        private void DoScalePopTween(Transform target, float scaleMult = 1.5f, float inTime = 0.07f, float outTime = 0.35f)
        {
            target.DOKill(true);
            var startingScale = target.localScale;
            
            DOTween.Sequence()
                .Append(target.transform.DOScale(startingScale * scaleMult, inTime).SetEase(Ease.OutQuad))
                .Append(target.transform.DOScale(startingScale, outTime).SetEase(Ease.InQuad));
        }
        
        private async UniTask DoTextValueTween(TMP_Text text, int fromValue, int toValue, float duration)
        {
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
    }
}