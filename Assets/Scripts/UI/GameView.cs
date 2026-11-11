using System;
using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
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
        private CancellationTokenSource _cts;
        
        [Inject]
        public void Construct(GameStateManager gameStateManager, MapService mapService)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.StateChanged += OnStateChanged;
            _mapService = mapService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;
                
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Game)
            {
                ResetCts();

                _diceUiSliding.HideInstant();
                _controlSliding.HideInstant();
                
                _selectionPanelCg.DOKill();
                _selectionPanelCg.Hide();
                
                gameObject.SetActive(true);
                ShowGameUiAsync(_cts.Token).Forget();
            }
            else if (state == GameState.GameResult)
            {
                ResetCts();
                HideGameUiAsync(_cts.Token).Forget();
            }
            else
            {
                _cts?.Cancel();
                
                _diceUiSliding.HideInstant();
                _controlSliding.HideInstant();
                
                _selectionPanelCg.DOKill();
                _selectionPanelCg.Hide();
                
                gameObject.SetActive(false);
            }
        }

        private void ResetCts()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new();
        }

        private async UniTaskVoid ShowGameUiAsync(CancellationToken token)
        {
            ShowBottomPanelsAsync(token).Forget();

            var isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeMap), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;
            
            _mapService.RequestMap();
        }
        
        private async UniTask ShowBottomPanelsAsync(CancellationToken token)
        {
            _controlSliding.ShowAsync().Forget();
            
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenPanels), cancellationToken: token);
            
            await _diceUiSliding.ShowAsync();
            
            await _selectionPanelCg.DOFade(1f, _selectionPanelFadeTime)
                .WithCancellation(token);
                
            _selectionPanelCg.interactable = true;
            _selectionPanelCg.blocksRaycasts = true;
        }

        private async UniTaskVoid HideGameUiAsync(CancellationToken token)
        {
            _selectionPanelCg.interactable = false;

            var isCancelled = await UniTask.WhenAll(
                _selectionPanelCg.DOFade(0f, _selectionPanelFadeTime).WithCancellation(token),
                _diceUiSliding.HideAsync(),
                _controlSliding.HideAsync()
            ).SuppressCancellationThrow();

            if (isCancelled) return;

            _selectionPanelCg.Hide();
            gameObject.SetActive(false);
        }
    }
}