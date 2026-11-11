using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _exitButton;
        
        private GameStateManager _gameStateManager;
        private Bootstrap _bootstrap;
        
        #region DI
        [Inject]
        public void Construct(GameStateManager gameStateManager, Bootstrap bootstrap)
        {
            _gameStateManager = gameStateManager;
            _bootstrap = bootstrap;
            
            _gameStateManager.StateChanged += OnStateChanged;
            OnStateChanged(_gameStateManager.CurrentState);
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;
        }

        private void Start()
        {
            _playButton.onClick.AddListener(_bootstrap.StartGame);
            _exitButton.onClick.AddListener(Application.Quit);
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                gameObject.SetActive(true);
                ShowMenuUiAsync().Forget();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid ShowMenuUiAsync()
        {
            if (_cg != null)
                _cg.Show();
        }
    }
}