using System;
using System.Collections.Generic;
using _Scripts.Dices;
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
        
        [Header("SFX")]
        [SerializeField] private string _addPointsSfx = "AddPoints";
        [SerializeField] private string _triggerMultSfx = "TriggerMult";
        [SerializeField] private string _addScoreSfx = "AddScore";
        
        private ISfxPlayer _sfxPlayer;
        private ScoreManager _scoreManager;
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, ScoreManager  scoreManager)
        {
            _sfxPlayer = sfxPlayer;
            _scoreManager = scoreManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public async UniTask PlaySequenceAsync(List<Dice> dices, ScoreCalculationResult scoreResult)
        {
            await AddPointsAsync(dices, scoreResult.Data.BasePoints);
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeMult));
            TriggerMult(scoreResult.TotalScore);
            await UniTask.Delay(TimeSpan.FromSeconds(_delayAfterMult));
            ApplyFinalScore(scoreResult.TotalScore);
        }
        
        private async UniTask<int> AddPointsAsync(List<Dice> dices, int basePoints)
        {
            var currentPoints = basePoints;
            for (int i = 0; i < dices.Count; i++)
            {
                var dice = dices[i];
                if (dice.Value <= 0) continue;
                
                currentPoints += dice.Value;
                _pointsText.text = currentPoints.ToString();
                
                PlayPointsAddFx(dice);
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerDice));
            }
            return currentPoints;
        }

        private void TriggerMult(int totalPoints)
        {
            _pointsText.text = totalPoints.ToString();
            
            PlayMultTriggerFx();
            
            _multText.text = "1";
        }
        
        private void PlayPointsAddFx(Dice dice)
        {
            DoScalePopTween(dice.transform);
            DoScalePopTween(_pointsText.transform, 1.8f);
            _sfxPlayer?.Play(_addPointsSfx, dice.transform.position);
        }

        private void PlayMultTriggerFx()
        {
            DoScalePopTween(_multText.transform);
            DoScalePopTween(_pointsText.transform, 3f);
            _sfxPlayer?.Play(_triggerMultSfx);
        }

        private void DoScalePopTween(Transform target, float scaleMult = 1.5f)
        {
            target.DOKill(true);
            var startingScale = target.localScale;
            
            DOTween.Sequence()
                .Append(target.transform.DOScale(startingScale * scaleMult, 0.05f).SetEase(Ease.OutQuad))
                .Append(target.transform.DOScale(startingScale, 0.3f).SetEase(Ease.InQuad));
        }

        private void ApplyFinalScore(int totalScore)
        {
            _scoreManager.AddScore(totalScore);
            _sfxPlayer?.Play(_addScoreSfx);
        }
    }
}