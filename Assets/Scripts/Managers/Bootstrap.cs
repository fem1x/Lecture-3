using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Managers
{
    public class Bootstrap : IInitializable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly DiceSpawner _diceSpawner;
        private readonly DiceTableController _diceTableController;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly DiceStatePanelView _diceStatePanelView;
        private readonly LevelService _levelService;
        private readonly MapService _mapService;
        private readonly GameStateManager _gameStateManager;
        private readonly ResourceManager _resourceManager;
        private readonly ScoreManager _scoreManager;
        private readonly DiceRoller _diceRoller;

        [Inject]
        public Bootstrap(
            DiceSpawner diceSpawner, 
            DiceTableController diceTableController, 
            ISfxPlayer sfxPlayer,
            DiceStatePanelView diceStatePanelView,
            LevelService levelService,
            MapService mapService,
            GameStateManager gameStateManager,
            ResourceManager resourceManager,
            ScoreManager scoreManager,
            DiceRoller diceRoller)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _sfxPlayer = sfxPlayer;
            _diceStatePanelView = diceStatePanelView;
            _levelService = levelService;
            _mapService = mapService;
            _gameStateManager = gameStateManager;
            _resourceManager = resourceManager;
            _scoreManager = scoreManager;
            _diceRoller = diceRoller;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Initialize()
        {
            _gameStateManager.EnterMenuState();
            _sfxPlayer.Initialize();
        }

        public void StartGame()
        {
            _gameStateManager.EnterGameState();
            
            _diceRoller.SetCanRoll(false);
            _resourceManager.ClearResources();
            _scoreManager.ClearScore();
            
            _levelService.InitializeLevelsOrder();
            _mapService.Initialize();
            
            var dices = _diceTableController.AllDices;
            if (dices == null || dices.Count == 0)
                dices = CreateNewDices();
            
            ResetDicesData(dices);
            ResetDicesState(dices);
            _levelService.SelectFirstLevel();
        }

        private void ResetDicesData(List<Dice> dices)
        {
            foreach (var dice in dices)
                dice.Data.ResetToDefault();
        }

        private List<Dice> CreateNewDices()
        {
            var dices = _diceSpawner.SpawnDices();
            _diceTableController.InitDiceList(dices);
            _diceStatePanelView.InitDiceStateViews(dices);

            return dices;
        }

        private void ResetDicesState(List<Dice> dices)
        {
            _diceSpawner.ResetDicesToSpawn(dices);
            _diceTableController.SetPendingDices(dices);
        }
    }
}