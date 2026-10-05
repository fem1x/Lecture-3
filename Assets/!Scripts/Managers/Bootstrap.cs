using System.Collections.Generic;
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
        private readonly ScoreManager _scoreManager;
        private readonly LevelFlowController _levelFlowController;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly CurrentCombinationView _currentCombinationView;
        private readonly DiceStatePanelView _diceStatePanelView;

        [Inject]
        public Bootstrap(
            DiceSpawner diceSpawner, 
            DiceTableController diceTableController, 
            ResourceManager resourceManager,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer,
            CurrentCombinationView currentCombinationView,
            DiceStatePanelView diceStatePanelView)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
            _currentCombinationView = currentCombinationView;
            _diceStatePanelView = diceStatePanelView;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Initialize()
        {
            _sfxPlayer.Initialize();
            
            var dices = _diceSpawner.SpawnDices();
            _diceSpawner.ResetDicesToSpawn(dices);
            _diceTableController.InitDiceList(dices);
            _diceStatePanelView.InitDiceStateViews(dices);
            _currentCombinationView.Init(dices);
            
            _levelFlowController.StartGame();
        }
    }
}