using System;
using UnityEngine;

namespace _Scripts.Managers
{
    public class ScoreManager
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int, int> OnScoreChanged; //(currentScore, scoreQuota)
        public event Action OnQuotaReached;
        
        public int CurrentScore {get; private set; }
        public int ScoreQuota {get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void SetQuota(int score)
        {
            ScoreQuota = score;
        }

        public void AddScore(int score)
        {
            if (score <= 0)
            {
                Debug.LogWarning("Score must be > 0!");
                return;
            }
            
            CurrentScore += score;
            OnScoreChanged?.Invoke(CurrentScore, ScoreQuota);
            
            if (HasEnoughScore)
                OnQuotaReached?.Invoke();
        }

        public void ResetScore()
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore, ScoreQuota);
        }
        
        private bool HasEnoughScore => CurrentScore >= ScoreQuota;
    }
}