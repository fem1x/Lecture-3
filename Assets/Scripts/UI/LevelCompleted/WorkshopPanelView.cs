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
        [SerializeField] private RopeButtonView _upgradeRopeButton;

        [Header("Panels")]
        [SerializeField] private DiceRepairPanelView _diceRepairPanel;
        [SerializeField] private CharmConveyorView _charmConveyor;
        [SerializeField] private UpgradePanelView _upgradePanel;
        [SerializeField] private float _conveyorOpenDelay = 0.2f;
        [SerializeField] private float _ropeShowDelay = 0.15f;

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

        internal SlidingPanelView FirstMainSlide => _sequenceController.FirstMainSlide;

        public event Action OnNextLevelRequested;

        private CancellationTokenSource _transitionCts;
        private bool _isUpgradeMode;
        private bool _conveyorInitializedThisVisit;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => HideInstant();

        private void OnEnable()
        {
            _nextLevelButton.onClick.AddListener(HandleNextLevelButtonClick);
            _upgradeRopeButton.OnPulled += HandleUpgradeToggleClick;
            _conveyorService.OnChoiceCompleted += HandleChoiceCompleted;
        }

        private void OnDisable()
        {
            _nextLevelButton.onClick.RemoveListener(HandleNextLevelButtonClick);
            _upgradeRopeButton.OnPulled -= HandleUpgradeToggleClick;
            _conveyorService.OnChoiceCompleted -= HandleChoiceCompleted;
            CancelTransition();
        }

        internal void PrepareOpening()
        {
            _sequenceController.gameObject.SetActive(true);
            gameObject.SetActive(true);

            foreach (Transform panel in transform)
                panel.gameObject.SetActive(true);

            CancelTransition();
            _isUpgradeMode = false;
            _conveyorInitializedThisVisit = false;

            _sequenceController.HideUpgradeInstant();
            _sequenceController.HideMainInstant();
            _upgradeRopeButton.gameObject.SetActive(false);

            _charmConveyor.InstantCloseShutter();
            _nextLevelButton.interactable = false;

            _diceRepairPanel.SetRepairMode(true);
            _slidingPanel.ShowInstant();
        }

        internal UniTask RevealContentsAsync(float openingDuration, CancellationToken ct)
        {
            return UniTask.WhenAll(
                _sequenceController.ShowMainAsync(_diceRepairPanel, ct),
                InitConveyorAfterDelayAsync(Mathf.Min(_conveyorOpenDelay, openingDuration * 0.72f), ct),
                ShowRopeAfterDelayAsync(Mathf.Min(_ropeShowDelay, openingDuration * 0.88f), ct)).AttachExternalCancellation(ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            ResetVisit();
            _sequenceController.HideUpgradeInstant();
            _upgradeRopeButton.RetractAsync(ct).Forget();
            await _slidingPanel.HideAsync(ct);
        }

        internal void CancelOpening()
        {
            ResetVisit();
            _sequenceController.HideMainInstant();
            HideInstant();
        }

        private void ResetVisit()
        {
            CancelTransition();
            _isUpgradeMode = false;
            _conveyorInitializedThisVisit = false;

            _charmConveyor.InstantCloseShutter();
            _charmConveyor.ClearConveyorView();
            _diceRepairPanel.SetRepairMode(false);
        }

        public void HideInstant()
        {
            _upgradeRopeButton.gameObject.SetActive(false);
            _slidingPanel.HideInstant();
            _sequenceController.HideUpgradeInstant();
        }

        private async UniTask ShowRopeAfterDelayAsync(float delay, CancellationToken ct)
        {
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();

            _upgradeRopeButton.gameObject.SetActive(true);
            _upgradeRopeButton.CollapseToAnchor();
        }

        private async UniTask InitConveyorAfterDelayAsync(float delay, CancellationToken ct)
        {
            if (_conveyorInitializedThisVisit) return;

            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true, cancellationToken: ct);

            ct.ThrowIfCancellationRequested();

            if (_conveyorInitializedThisVisit) return;
            _conveyorInitializedThisVisit = true;

            InitializeConveyorPanel();
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
                        _sequenceController.ShowMainAsync(_diceRepairPanel, ct));
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
