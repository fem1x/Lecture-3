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
        [SerializeField] private CanvasGroup _controlsGroup;
        [SerializeField] private RectTransform _controlsRect;
        [SerializeField] private StaggeredRevealView _controlsReveal;

        [SerializeField] private float _delayBetweenPanels = 0.3f;
        [SerializeField] private float _selectionPanelFadeTime = 0.5f;
        [SerializeField] private float _delayBeforeMap = 0.5f;
        
        private GameStateManager _gameStateManager;
        private MapService _mapService;
        private CancellationTokenSource _cts;
        private Vector3 _controlsScale;
        private Sequence _controlsTween;
        private bool _controlsInitialized;
        
        [Inject]
        public void Construct(GameStateManager gameStateManager, MapService mapService)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.StateChanged += OnStateChanged;
            _mapService = mapService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Awake() => EnsureControlsInitialized();

        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;
                
            _cts?.Cancel();
            _cts?.Dispose();
            _controlsTween?.Kill();
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Game)
            {
                ResetCts();

                _diceUiSliding.HideInstant();
                _controlSliding.HideInstant();
                
                HideControlsInstant();
                
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
                
                HideControlsInstant();
                
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
            _controlSliding.ShowAsync(token).Forget();
            
            await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenPanels), cancellationToken: token);
            
            await _diceUiSliding.ShowAsync(token);
            
            _selectionPanelCg.Show();
            _controlsReveal.Show();
            _controlsGroup.alpha = 0f;
            _controlsRect.localScale = _controlsScale * 0.9f;
            _controlsGroup.interactable = false;
            _controlsGroup.blocksRaycasts = false;
            _controlsTween?.Kill();
            _controlsTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
                .Join(_controlsGroup.DOFade(1f, _selectionPanelFadeTime).SetEase(Ease.OutQuad).SetLink(gameObject))
                .Join(_controlsRect.DOScale(_controlsScale, _selectionPanelFadeTime).SetEase(Ease.OutBack).SetLink(gameObject));
            await _controlsTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token);

            _controlsGroup.interactable = true;
            _controlsGroup.blocksRaycasts = true;
        }

        private async UniTaskVoid HideGameUiAsync(CancellationToken token)
        {
            _controlsGroup.interactable = false;
            _controlsGroup.blocksRaycasts = false;
            _controlsReveal.Restore();
            _controlsTween?.Kill();
            _controlsGroup.DOKill();
            _controlsRect.DOKill();

            var isCancelled = await UniTask.WhenAll(
                _controlsGroup.DOFade(0f, _selectionPanelFadeTime).SetUpdate(true).SetLink(gameObject).WithCancellation(token),
                _diceUiSliding.HideAsync(token),
                _controlSliding.HideAsync(token)
            ).SuppressCancellationThrow();

            if (isCancelled) return;

            HideControlsInstant();
            gameObject.SetActive(false);
        }

        private void HideControlsInstant()
        {
            EnsureControlsInitialized();
            _controlsReveal.Restore();
            _controlsTween?.Kill();
            _controlsGroup.DOKill();
            _controlsRect.DOKill();
            _controlsRect.localScale = _controlsScale;
            _controlsGroup.Hide();
            _selectionPanelCg.Show();
        }

        private void EnsureControlsInitialized()
        {
            if (_controlsInitialized) return;
            _controlsScale = _controlsRect.localScale;
            _controlsInitialized = true;
        }
    }
}
