using System;
using System.Threading;
using _Scripts.Dices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class WorkshopPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("References")]
        [SerializeField] private SlidingWindowView _slidingWindow;
        [SerializeField] private Button _nextLevelButton;
        [Space]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private CharmConveyorView _charmConveyor;

        [Header("Dice Anchor")]
        [SerializeField] private RectTransform _diceSlotRect;
        [SerializeField] private float _diceMoveDuration = 0.35f;
        [SerializeField] private DG.Tweening.Ease _diceEase = DG.Tweening.Ease.OutBack;

        #region DI
        private DiceStatePanelView _dicePanelView;

        [Inject]
        public void Construct(DiceStatePanelView dicePanelView)
        {
            _dicePanelView = dicePanelView;
        }
        #endregion

        public event Action OnNextLevelRequested;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => HideInstant();

        private void OnEnable() => _nextLevelButton.onClick.AddListener(HandleNextLevelButtonClick);
        private void OnDisable() => _nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClick);

        private void HandleNextLevelButtonClick() => OnNextLevelRequested?.Invoke();

        public async UniTask ShowAsync(CancellationToken ct)
        {
            _nextLevelButton.interactable = false;

            var targetPosition = _diceSlotRect.position;

            await UniTask.WhenAll(
                _slidingWindow.ShowAsync(ct),
                _dicePanelView.MoveToPositionTween(targetPosition, _diceMoveDuration, _diceEase)
                    .ToUniTask(cancellationToken: ct)
            );

            _dicePanelView.SetRepairMode(true);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            _dicePanelView.SetRepairMode(false);

            await UniTask.WhenAll(
                _slidingWindow.HideAsync(ct),
                _dicePanelView.ResetPositionTween(_diceMoveDuration, _diceEase).ToUniTask(cancellationToken: ct)
            );
        }

        public void HideInstant() => _slidingWindow.HideInstant();
    }
}