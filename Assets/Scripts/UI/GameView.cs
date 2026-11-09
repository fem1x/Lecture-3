using System;
using System.Threading.Tasks;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class GameView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Elements")] 
        [SerializeField] private SlidingPanelView _diceUiSliding;
        [SerializeField] private SlidingPanelView _controlSliding;

        [SerializeField] private float _delayBeforeMap = 0.5f;
        
        private GameStateManager _gameStateManager;
        private MapService _mapService;
        
        [Inject]
        public void Construct(GameStateManager gameStateManager, MapService mapService)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.StateChanged += OnStateChanged;
            _mapService = mapService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy() => _gameStateManager.StateChanged -= OnStateChanged;

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Game)
                ShowGameUiAsync().Forget();
        }

        private async UniTaskVoid ShowGameUiAsync()
        {
            await SlidePanelsAsync();
            _mapService.RequestMap();
        }

        private async UniTask SlidePanelsAsync()
        {
            _diceUiSliding.ShowAsync().Forget();
            _controlSliding.ShowAsync().Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeMap));
        }
    }
}