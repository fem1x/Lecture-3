using System;
using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class LevelFlowController : IDisposable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int, int> OnLevelStarted; //levelIndex, quota
        public event Action OnLevelCompleted;
        public event Action OnGameWon;
        public event Action OnGameOver;

		private readonly LevelsListConfig _levelsConfig;
        private readonly ScoreManager _scoreManager;
        private readonly ResourceManager _resourceManager;
        private readonly DiceTableController _tableController;
        private readonly DiceSpawner _diceSpawner;
        
        [Inject]
        public LevelFlowController(
            LevelsListConfig levelsConfig,
            ScoreManager scoreManager,
            ResourceManager resourceManager,
            DiceTableController tableController,
            DiceSpawner diceSpawner)
        {
            _levelsConfig = levelsConfig;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
            _tableController = tableController;
            _diceSpawner = diceSpawner;
            
            _scoreManager.OnQuotaReached += HandleQuotaReached;
        }
        
        private int _currentLevelIndex = 0;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Dispose() => _scoreManager.OnQuotaReached -= HandleQuotaReached;
        
        public void StartGame()
        {
            _currentLevelIndex = 0;
            StartCurrentLevel();
        }
        
        public bool TryStartNextLevel()
        {
            if (_currentLevelIndex < _levelsConfig.LevelsList.Count)
            {
                StartCurrentLevel();
                return true;
            }

            OnGameWon?.Invoke();
            return false;
        } 
        
        private void StartCurrentLevel()
        {
            if (_currentLevelIndex >= _levelsConfig.LevelsList.Count)
            {
                OnGameWon?.Invoke();
                return;
            }

            var currentLevel = _levelsConfig.LevelsList[_currentLevelIndex];
            
            _scoreManager.ResetScore();
            _scoreManager.SetQuota(currentLevel.ScoreQuota);

            _resourceManager.ResetForNewLevel();

            var allDices = _tableController.AllDices;
            _tableController.ClearSelection();
            _tableController.ActivateAllDices(allDices);
            _diceSpawner.ResetDicesToSpawn(allDices);
            _tableController.SetPendingDices(allDices);
            
            OnLevelStarted?.Invoke(_currentLevelIndex + 1, currentLevel.ScoreQuota);
        }
        
        public void CheckDefeatCondition()
        {
            if (!_scoreManager.HasEnoughScore && _resourceManager.DiceCount <= 0 && !_tableController.HasPendingDices)
            {
                OnGameOver?.Invoke();
            }
        }
        
        private void HandleQuotaReached()
        {
            _currentLevelIndex++;
            OnLevelCompleted?.Invoke();
        }
    }
}