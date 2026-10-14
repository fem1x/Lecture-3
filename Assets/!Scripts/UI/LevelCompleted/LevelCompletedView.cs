using System;
using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [Header("Panels")]
    [SerializeField] private SlidingWindowView _slidingWindow;
    [SerializeField] private LevelRewardsView _rewardsView;
    [SerializeField] private WorkshopPanelView _workshopView;

    [Header("Dice Slot")]
    [SerializeField] private RectTransform _diceSlotRect;

    [Header("Transition")]
    [SerializeField] private float _diceMoveDuration = 0.35f;
    [SerializeField] private float _pageFadeDuration = 0.2f;
    [SerializeField] private Ease _diceEase = Ease.OutBack;

    #region DI
    private LevelFlowController _levelFlowController;
    private DiceStatePanelView _dicePanelView;

    [Inject]
    public void Construct(LevelFlowController levelFlowController, DiceStatePanelView dicePanelView)
    {
        _levelFlowController = levelFlowController;
        _dicePanelView = dicePanelView;
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Start() => HideInstant();

    private void OnEnable()
    {
        _levelFlowController.OnLevelCompleted += HandleLevelCompleted;
        _rewardsView.OnWorkshopRequested += HandleWorkshopRequested;
        _workshopView.OnNextLevelRequested += HandleNextLevelRequested;
    }

    private void OnDisable()
    {
        _levelFlowController.OnLevelCompleted -= HandleLevelCompleted;
        _rewardsView.OnWorkshopRequested -= HandleWorkshopRequested;
        _workshopView.OnNextLevelRequested -= HandleNextLevelRequested;
    }
    
    private void HandleWorkshopRequested() => SwitchToWorkshopPanelAsync().Forget();
    private void HandleNextLevelRequested() => CompleteAndHideAsync().Forget();

    private void HandleLevelCompleted(TokenRewardData rewardData)
    {
        _rewardsView.Setup(rewardData);
        ShowRewardsPanelAsync().Forget();
    }

    private async UniTaskVoid ShowRewardsPanelAsync()
    {
        var ct = destroyCancellationToken;

        _rewardsView.SetVisible(true);
        _workshopView.SetVisible(false);

        await _slidingWindow.ShowAsync(ct);
    }

    private async UniTaskVoid SwitchToWorkshopPanelAsync()
    {
        var ct = destroyCancellationToken;

        var rewardsCanvas = _rewardsView.GetComponent<CanvasGroup>();
        var workshopCanvas = _workshopView.GetComponent<CanvasGroup>();

        Canvas.ForceUpdateCanvases();

        await DOTween.Sequence()
            .SetTarget(transform)
            .SetUpdate(true)
            .Join(rewardsCanvas.DOFade(0f, _pageFadeDuration))
            .Join(_dicePanelView.MoveToAnchorTween(_diceSlotRect, _diceMoveDuration, _diceEase))
            .AppendCallback(() =>
            {
                _rewardsView.SetVisible(false);
                _workshopView.SetVisible(true);
                _workshopView.SetNextLevelInteractable(false);
                _dicePanelView.SetRepairMode(true);
            })
            .Append(workshopCanvas.DOFade(1f, _pageFadeDuration))
            .WithCancellation(ct);
    }

    private async UniTaskVoid CompleteAndHideAsync()
    {
        var ct = destroyCancellationToken;

        _dicePanelView.SetRepairMode(false);

        await DOTween.Sequence()
            .SetTarget(transform)
            .SetUpdate(true)
            .Join(_dicePanelView.ResetPositionTween(_diceMoveDuration, _diceEase))
            .Join(_slidingWindow.HideTween())
            .WithCancellation(ct);
        
        _levelFlowController.TryStartNextLevel();
    }
    
    private void HideInstant()
    {
        _slidingWindow.HideInstant();
        _rewardsView.SetVisible(false);
        _workshopView.SetVisible(false);
        _dicePanelView.SetRepairMode(false);
    }
}