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
        [SerializeField] private WorkshopSequenceController _sequenceController;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _upgradeToggleButton;

        [Header("Panels & Timings")]
        [SerializeField] private CharmConveyorView _charmConveyor;
        [SerializeField] private UpgradePanelView _upgradePanel;
        [SerializeField] private float _upgradeAppearDelay = 0.15f;
        [SerializeField] private float _mainAppearDelay = 0.15f;

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
            _sequenceController.HideUpgradeInstant();
            _sequenceController.HideMainInstant();

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

            await _sequenceController.ShowMainAsync(ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            CancelTransition();
            _isUpgradeMode = false;

            _charmConveyor.InstantCloseShutter();
            _charmConveyor.ClearConveyorView();
            _dicePanelView.SetRepairMode(false);
            _sequenceController.HideUpgradeInstant();

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
            _sequenceController.HideUpgradeInstant();
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
                    _upgradePanel.BindSections();

                    await RunOverlappingAsync(
                        () => _sequenceController.HideMainAsync(ct),
                        () => _sequenceController.ShowUpgradeAsync(ct),
                        _upgradeAppearDelay,
                        ct);

                    _isUpgradeMode = true;
                }
                else
                {
                    InitializeConveyorPanel();
                    _dicePanelView.SetRepairMode(true);

                    await RunOverlappingAsync(
                        () => _sequenceController.HideUpgradeAsync(ct),
                        () => _sequenceController.ShowMainAsync(ct),
                        _mainAppearDelay,
                        ct);

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

        private async UniTask RunOverlappingAsync(
            Func<UniTask> firstTaskFactory,
            Func<UniTask> secondTaskFactory,
            float secondDelay,
            CancellationToken ct)
        {
            var firstTask = firstTaskFactory();
            var secondTask = StartDelayedAsync(secondTaskFactory, secondDelay, ct);

            await UniTask.WhenAll(firstTask, secondTask);
        }

        private async UniTask StartDelayedAsync(Func<UniTask> taskFactory, float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();
            await taskFactory();
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