using System;
using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class GameResultsView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private SlidingPanelView _slidingPanel;
        [SerializeField] private VictoryShowcaseAnimationView _showcaseView;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private float _delayBeforeShow = 0.8f;
        [SerializeField] private float _delayBeforeShowcase = 0.4f;

        private GameStateManager _gameStateManager;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(GameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.StateChanged += OnStateChanged;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy()
        {
            _gameStateManager.StateChanged -= OnStateChanged;
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.GameResult)
            {
                ResetCts();

                SetupView(_gameStateManager.LastGameResult);
                
                _cg.DOKill();
                _cg.Hide();
                _slidingPanel?.HideInstant();
                _showcaseView?.HideInstant();
                
                gameObject.SetActive(true);
                ShowAsync(_cts.Token).Forget();
            }
            else
            {
                _cts?.Cancel();
                HideInstant();
            }
        }

        private void ResetCts()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new();
        }

        private void SetupView(GameEndType resultType)
        {
            _titleText.text = resultType == GameEndType.Victory ? "ПОБЕДА!" : "ПОРАЖЕНИЕ";
        }

        private async UniTaskVoid ShowAsync(CancellationToken token)
        {
            var isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeShow), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;

            _cg.DOKill();
            _cg.Show();
            _slidingPanel?.ShowAsync();

            ShowShowcaseWithDelayAsync(token).Forget();
        }

        private async UniTaskVoid ShowShowcaseWithDelayAsync(CancellationToken token)
        {
            var isCancelled = await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeShowcase), cancellationToken: token).SuppressCancellationThrow();
            if (isCancelled) return;

            _showcaseView.PlayShowAsync(token).Forget();
        }
        
        private void HideInstant()
        {
            _cg.DOKill();
            _cg.Hide();
            _slidingPanel?.HideInstant();
            _showcaseView?.HideInstant();
            gameObject.SetActive(false);
        }
    }
}