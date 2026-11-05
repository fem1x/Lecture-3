using _Scripts.Managers;
using _Scripts.Utility;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class ScoreView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _quotaText;
        [SerializeField] private float _scoreTweenTime = 0.6f;
        
        private int _displayedValue;
        private Tween _currentTween;
        private ScoreManager _scoreManager;
        [Inject]
        public void Construct(ScoreManager scoreManager)
        {
            _scoreManager = scoreManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable()
        {
            _scoreManager.OnScoreChanged += UpdateText;
            InstantUpdateText(_scoreManager.CurrentScore, _scoreManager.ScoreQuota);
        }

        private void OnDisable() => _scoreManager.OnScoreChanged -= UpdateText;


        private void InstantUpdateText(int currentValue, int quotaValue)
        {
            KillCurrentTween();
            _displayedValue = currentValue;
            
            _quotaText.text = quotaValue.ToString();
            _scoreText.text = currentValue.ToString();
        }
        
        private void UpdateText(int currentValue, int quotaValue) 
        {
            _quotaText.text = quotaValue.ToString();
            
            if (currentValue == 0)
            {
                InstantUpdateText(0, quotaValue);
                return;
            }

            var fromValue = _displayedValue;
            _displayedValue = currentValue;

            KillCurrentTween();
            _currentTween = Utils.DoTextValueTween(_scoreText, fromValue, currentValue, _scoreTweenTime);
        }
        
        private void KillCurrentTween()
        {
            if (_currentTween != null && _currentTween.IsActive())
            {
                _currentTween.Kill();
                _currentTween = null;
            }
            _scoreText.DOKill();
        }
    }
}