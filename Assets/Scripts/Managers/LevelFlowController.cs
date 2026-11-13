using System;
using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class LevelFlowController
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<TokenRewardData> OnLevelCompleted;

        private LevelConfig _currentLevel;
        
        private readonly LevelService _levelService;
        private readonly ScoreManager _scoreManager;
        private readonly ResourceManager _resourceManager;
        private readonly DiceTableController _tableController;
        private readonly DiceSpawner _diceSpawner;
        private readonly RewardCalculator _rewardCalculator;
        private readonly RepairTokensManager _repairTokensManager;
        private readonly CodeLockService _codeLockService;
        private readonly DiceRoller _diceRoller;
        private readonly GameStateManager _gameStateManager;
        
        #region DI
        [Inject]
        public LevelFlowController(
            LevelService levelService,
            ScoreManager scoreManager,
            ResourceManager resourceManager,
            DiceTableController tableController,
            DiceSpawner diceSpawner,
            RewardCalculator rewardCalculator,
            RepairTokensManager repairTokensManager,
            CodeLockService codeLockService,
            DiceRoller diceRoller,
            GameStateManager gameStateManager)
        {
            _levelService = levelService;
            _scoreManager = scoreManager;
            _resourceManager = resourceManager;
            _tableController = tableController;
            _diceSpawner = diceSpawner;
            _rewardCalculator = rewardCalculator;
            _repairTokensManager = repairTokensManager;
            _codeLockService = codeLockService;
            _diceRoller = diceRoller;
            _gameStateManager = gameStateManager;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====


        public void StartLevel()
        {
            _currentLevel = _levelService.CurrentLevel;
            
            _scoreManager.ResetScore();
            _scoreManager.SetQuota(_currentLevel.ScoreQuota);

            _resourceManager.ResetForNewLevel();

            var allDices = _tableController.AllDices;
            _tableController.ClearSelection();
            _tableController.ActivateAllDices(allDices);
            _diceSpawner.ResetDicesToSpawn(allDices);
            _tableController.SetPendingDices(allDices);
            
            _codeLockService.SetLock(_currentLevel);
            
            _diceRoller.SetCanRoll(true);
        }
        
        public void CheckDefeatCondition()
        {
            if (!_scoreManager.HasEnoughScore && _resourceManager.DiceCount <= 0 && !_tableController.HasPendingDices)
            {
                _diceRoller.SetCanRoll(false);
                _gameStateManager.EnterGameResultState(GameEndType.Defeat);
            }
        }
        
        public void CompleteLevel()
        {
            _diceRoller.SetCanRoll(false);
            var rewardData = _rewardCalculator.GetTokenRewards(_currentLevel.TokensReward);
            _repairTokensManager.AddTokens(rewardData.TotalTokens);
            
            if (_levelService.TrySelectNextLevel())
                OnLevelCompleted?.Invoke(rewardData);
            else
                _gameStateManager.EnterGameResultState(GameEndType.Victory);
        }
    }
}