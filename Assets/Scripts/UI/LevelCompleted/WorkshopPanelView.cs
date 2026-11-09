using System;
using System.Threading;
using _Scripts.Charms;
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
        [SerializeField] private SlidingPanelView _slidingPanel;
        [SerializeField] private Button _nextLevelButton;
        [Space]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private CharmConveyorView _charmConveyor;

        [Header("Dice Anchor")]
        [SerializeField] private RectTransform _diceSlotRect;

        #region DI
        private DiceStatePanelView _dicePanelView;
        private CharmsRewardGenerator _rewardGenerator;
        private CharmsConveyorService _conveyorService;

        [Inject]
        public void Construct(
            DiceStatePanelView dicePanelView,
            CharmsRewardGenerator rewardGenerator,
            CharmsConveyorService conveyorService)
        {
            _dicePanelView = dicePanelView;
            _rewardGenerator = rewardGenerator;
            _conveyorService = conveyorService;
        }
        #endregion

        public event Action OnNextLevelRequested;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => HideInstant();

        private void OnEnable()
        {
            _nextLevelButton.onClick.AddListener(HandleNextLevelButtonClick);
            _conveyorService.OnChoiceCompleted += HandleChoiceCompleted;
        }

        private void OnDisable()
        {
            _nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClick);
            _conveyorService.OnChoiceCompleted -= HandleChoiceCompleted;
        }

        private void HandleNextLevelButtonClick() => OnNextLevelRequested?.Invoke();
        private void HandleChoiceCompleted() => _nextLevelButton.interactable = true;

        public async UniTask ShowAsync(CancellationToken ct)
        {
            _charmConveyor.InstantCloseShutter();
            _nextLevelButton.interactable = false;

            var panelTask = _slidingPanel.ShowAsync();

            await UniTask.WaitUntil(
                () => _diceSlotRect.position.y >= _dicePanelView.HolderY,
                cancellationToken: ct);

            _dicePanelView.GrabBy(_diceSlotRect);

            await panelTask;

            InitializeConveyorPanel();
            _dicePanelView.SetRepairMode(true);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            _charmConveyor.InstantCloseShutter();
            _charmConveyor.ClearConveyorView();
            _dicePanelView.SetRepairMode(false);

            var panelTask = _slidingPanel.HideAsync();

            await UniTask.WaitUntil(
                () => _diceSlotRect.position.y <= _dicePanelView.HolderY,
                cancellationToken: ct);

            _dicePanelView.Drop();

            await panelTask;
        }

        public void HideInstant() => _slidingPanel.HideInstant();

        private void InitializeConveyorPanel()
        {
            var rewardCharms = _rewardGenerator.GenerateCharmsRewards();
            _conveyorService.InitializeConveyor(rewardCharms);
        }
    }
}