using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Dices;
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
        private readonly ResourceManager _resourceManager;
        private readonly LevelFlowController _levelFlowController;
        private readonly ISfxPlayer _sfxPlayer;

        [Inject]
        public Bootstrap(
            DiceSpawner diceSpawner, 
            DiceTableController diceTableController, 
            ResourceManager resourceManager,
            LevelFlowController levelFlowController,
            ISfxPlayer sfxPlayer)
        {
            _diceSpawner = diceSpawner;
            _diceTableController = diceTableController;
            _resourceManager = resourceManager;
            _levelFlowController = levelFlowController;
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Initialize()
        {
            _sfxPlayer.Initialize();
            
            var dices = _diceSpawner.SpawnDices();
            _diceSpawner.ResetDicesToSpawn(dices);
            _diceTableController.InitDiceList(dices);
            
            _levelFlowController.StartGame();
        }
    }
}