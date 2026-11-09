using System;
using System.Threading.Tasks;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        [SerializeField] private CanvasGroup _selectionPanelCg;

        [SerializeField] private float _delayBetweenPanels = 0.3f;
        [SerializeField] private float _selectionPanelFadeTime = 0.5f;
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
            ShowBottomPanelsAsync().Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeMap));
            _mapService.RequestMap();
        }

        private async UniTask ShowBottomPanelsAsync()
        {
            _controlSliding.ShowAsync().Forget();
            await  UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenPanels));
            await _diceUiSliding.ShowAsync();
            await _selectionPanelCg.DOFade(1f, _selectionPanelFadeTime);
        }
    }
}