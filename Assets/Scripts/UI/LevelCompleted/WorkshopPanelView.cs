using System;
using System.Threading;
using _Scripts.Charms;
using _Scripts.UI.Workshop;
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
        [SerializeField] private Button _upgradeToggleButton;

        [Header("Panels")]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private UpgradePanelView _upgradePanel;
        [SerializeField] private CharmConveyorView _charmConveyor;

        [Header("Slides")]
        [SerializeField] private SlidingPanelView _diceRepairSlide;
        [SerializeField] private SlidingPanelView _charmConveyorSlide;
        [SerializeField] private SlidingPanelView _nextLevelSlide;
        [SerializeField] private SlidingPanelView _upgradeSlide;

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

        private CancellationTokenSource _transitionCts;
        private bool _isUpgradeMode;
        private bool _isTransitioning;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => HideInstant();

        private void OnEnable()
        {
            _nextLevelButton.onClick.AddListener(HandleNextLevelButtonClick);
            _upgradeToggleButton.onClick.AddListener(HandleUpgradeToggleClick);
            _conveyorService.OnChoiceCompleted += HandleChoiceCompleted;
        }

        private void OnDisable()
        {
            _nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClick);
            _upgradeToggleButton.onClick.RemoveListener(HandleUpgradeToggleClick);
            _conveyorService.OnChoiceCompleted -= HandleChoiceCompleted;
            CancelTransition();
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            _isUpgradeMode = false;
            _upgradeSlide.HideInstant();
            _charmConveyorSlide.HideInstant();
            _diceRepairSlide.HideInstant();
            _nextLevelSlide.HideInstant();

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

            await _charmConveyorSlide.ShowAsync();
            await _diceRepairSlide.ShowAsync();
            await _nextLevelSlide.ShowAsync();
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            CancelTransition();
            _isUpgradeMode = false;

            _charmConveyor.InstantCloseShutter();
            _charmConveyor.ClearConveyorView();
            _dicePanelView.SetRepairMode(false);
            _upgradeSlide.HideInstant();

            var panelTask = _slidingPanel.HideAsync();

            await UniTask.WaitUntil(
                () => _diceSlotRect.position.y <= _dicePanelView.HolderY,
                cancellationToken: ct);

            _dicePanelView.Drop();

            await panelTask;
        }

        public void HideInstant()
        {
            _slidingPanel.HideInstant();
            _upgradeSlide.HideInstant();
        }

        private void HandleNextLevelButtonClick() => OnNextLevelRequested?.Invoke();

        private void HandleChoiceCompleted() => _nextLevelButton.interactable = true;

        private void HandleUpgradeToggleClick()
        {
            if (_isTransitioning) return;

            CancelTransition();
            _transitionCts = new();

            ToggleUpgradeModeAsync(_transitionCts.Token).Forget();
        }

        private async UniTaskVoid ToggleUpgradeModeAsync(CancellationToken ct)
        {
            _isTransitioning = true;
            _upgradeToggleButton.interactable = false;

            try
            {
                if (!_isUpgradeMode)
                {
                    _dicePanelView.SetRepairMode(false);
                    _charmConveyor.InstantCloseShutter();

                    await _charmConveyorSlide.HideAsync();
                    await _diceRepairSlide.HideAsync();
                    await _nextLevelSlide.HideAsync();

                    ct.ThrowIfCancellationRequested();
                    _upgradePanel.BindSections();
                    await _upgradeSlide.ShowAsync();
                    _isUpgradeMode = true;
                }
                else
                {
                    await _upgradeSlide.HideAsync();
                    ct.ThrowIfCancellationRequested();

                    InitializeConveyorPanel();
                    _dicePanelView.SetRepairMode(true);

                    await _charmConveyorSlide.ShowAsync();
                    await _diceRepairSlide.ShowAsync();
                    await _nextLevelSlide.ShowAsync();

                    _isUpgradeMode = false;
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _upgradeToggleButton.interactable = true;
                _isTransitioning = false;
            }
        }

        private void InitializeConveyorPanel()
        {
            var rewardCharms = _rewardGenerator.GenerateCharmsRewards();
            _conveyorService.InitializeConveyor(rewardCharms);
        }

        private void CancelTransition()
        {
            _transitionCts?.Cancel();
            _transitionCts?.Dispose();
            _transitionCts = null;
        }
    }
}