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
        private readonly DiceNetPanelView _diceNetPanelView;
        private readonly CurrentCombinationView _currentCombinationView;

        [Inject]
        public Bootstrap(
            DiceSpawner diceSpawner, 
            DiceTableController diceTableController, 
            ResourceManager resourceManager,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer,
            DiceNetPanelView diceNetPanelView,
            CurrentCombinationView currentCombinationView)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
            _diceNetPanelView = diceNetPanelView;
            _currentCombinationView = currentCombinationView;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Initialize()
        {
            _sfxPlayer.Initialize();
            
            var dices = _diceSpawner.SpawnDices();
            _diceSpawner.ResetDicesToSpawn(dices);
            _diceTableController.InitDiceList(dices);
            _diceNetPanelView.InitNetViews(dices);
            _currentCombinationView.Init(dices);
            
            _levelFlowController.StartGame();
        }
    }
}