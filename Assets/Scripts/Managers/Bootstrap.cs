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

        [Inject]
        public Bootstrap(
            DiceSpawner diceSpawner, 
            DiceTableController diceTableController, 
            ISfxPlayer sfxPlayer,
            DiceStatePanelView diceStatePanelView,
            LevelService levelService,
            MapService mapService,
            GameStateManager gameStateManager)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _sfxPlayer = sfxPlayer;
            _diceStatePanelView = diceStatePanelView;
            _levelService = levelService;
            _mapService = mapService;
            _gameStateManager = gameStateManager;
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
            
            _levelService.InitializeLevelsOrder();
            _mapService.Initialize();
            
            var dices = _diceSpawner.SpawnDices();
            _diceSpawner.ResetDicesToSpawn(dices);
            _diceTableController.InitDiceList(dices);
            _diceStatePanelView.InitDiceStateViews(dices);
            
            _levelService.SelectFirstLevel();
        }
    }
}