using _Scripts.Managers;
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
        
        [Inject]
        public void Construct(GameStateManager gameStateManager,  Bootstrap bootstrap)
        {
            _gameStateManager = gameStateManager;
            _bootstrap = bootstrap;
            
            _gameStateManager.StateChanged += OnStateChanged;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy() => _gameStateManager.StateChanged -= OnStateChanged;
        private void Start()
        {
            _playButton.onClick.AddListener(_bootstrap.StartGame);
            _exitButton.onClick.AddListener(Application.Quit);
        }

        private void OnStateChanged(GameState state)
        {
            
        }
    }
}