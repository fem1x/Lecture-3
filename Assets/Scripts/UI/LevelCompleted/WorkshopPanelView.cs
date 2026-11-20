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

        [Header("Panels")]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private CharmConveyorView _charmConveyor;
        [SerializeField] private UpgradePanelView _upgradePanel;

        #region DI
        private CharmsRewardGenerator _rewardGenerator;
        private CharmsConveyorService _conveyorService;

        [Inject]
        public void Construct(
            CharmsRewardGenerator rewardGenerator,
            CharmsConveyorService conveyorService)
        {
            _rewardGenerator = rewardGenerator;
            _conveyorService = conveyorService;
        }
        #endregion

        public event Action OnNextLevelRequested;

        private CancellationTokenSource _transitionCts;
        private bool _isUpgradeMode;
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
            CancelTransition();
            _isUpgradeMode = false;
            _sequenceController.HideUpgradeInstant();
            _sequenceController.HideMainInstant();

            _charmConveyor.InstantCloseShutter();
            _nextLevelButton.interactable = false;

            InitializeConveyorPanel();
            _diceRepairPanel.SetRepairMode(true);

            await UniTask.WhenAll(
                _slidingPanel.ShowAsync(),
                _sequenceController.ShowMainAsync(ct));
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            CancelTransition();
            _isUpgradeMode = false;

            _charmConveyor.InstantCloseShutter();
            _charmConveyor.ClearConveyorView();
            _diceRepairPanel.SetRepairMode(false);
            _sequenceController.HideUpgradeInstant();

            await _slidingPanel.HideAsync();
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
            CancelTransition();
            _transitionCts = new();

            ToggleUpgradeModeAsync(_transitionCts.Token).Forget();
        }

        private async UniTaskVoid ToggleUpgradeModeAsync(CancellationToken ct)
        {
            _isUpgradeMode = !_isUpgradeMode;

            try
            {
                if (_isUpgradeMode)
                {
                    _diceRepairPanel.SetRepairMode(false);
                    _upgradePanel.BindSections();

                    await UniTask.WhenAll(
                        _sequenceController.HideMainAsync(ct),
                        _sequenceController.ShowUpgradeAsync(ct));
                }
                else
                {
                    _diceRepairPanel.SetRepairMode(true);

                    await UniTask.WhenAll(
                        _sequenceController.HideUpgradeAsync(ct),
                        _sequenceController.ShowMainAsync(ct));
                }
            }
            catch (OperationCanceledException) { }
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