using _Scripts.Managers;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class ScoreView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _scoreText;
        
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
            UpdateText(_scoreManager.CurrentScore, _scoreManager.ScoreQuota);
        }

        private void OnDisable() => _scoreManager.OnScoreChanged -= UpdateText;

        private void UpdateText(int current, int quota)
        {
            _scoreText.text = $"{current} / {quota}";
        }
    }
}