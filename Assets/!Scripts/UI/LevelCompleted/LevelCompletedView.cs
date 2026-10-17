using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [Header("Panels")]
    [SerializeField] private LevelRewardsView _rewardsView;
    [SerializeField] private WorkshopPanelView _workshopView;

    [Header("Backdrop")]
    [SerializeField] private float _backdropFadeDuration = 0.25f;
    [SerializeField] private CanvasGroup _backdropCanvasGroup;

    #region DI
    private LevelFlowController _levelFlowController;

    [Inject]
    public void Construct(LevelFlowController levelFlowController)
    {
        _levelFlowController = levelFlowController;
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Awake()
    {
        _backdropCanvasGroup = GetComponent<CanvasGroup>();
        HideInstant();
    }

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

    private void HandleLevelCompleted(TokenRewardData rewardData)
    {
        _rewardsView.Setup(rewardData);
        ShowFlowAsync().Forget();
    }

    private void HandleWorkshopRequested() => SwitchToWorkshopAsync().Forget();
    private void HandleNextLevelRequested() => CompleteAndHideAsync().Forget();

    private async UniTaskVoid ShowFlowAsync()
    {
        var ct = destroyCancellationToken;

        SetBackdropInteractable(true);
        _backdropCanvasGroup.DOKill();

        await UniTask.WhenAll(
            _backdropCanvasGroup.DOFade(1f, _backdropFadeDuration).SetUpdate(true).WithCancellation(ct),
            _rewardsView.ShowAsync(ct)
        );
    }

    private async UniTaskVoid SwitchToWorkshopAsync()
    {
        var ct = destroyCancellationToken;

        await _rewardsView.HideAsync(ct);
        await _workshopView.ShowAsync(ct);
    }

    private async UniTaskVoid CompleteAndHideAsync()
    {
        var ct = destroyCancellationToken;

        _backdropCanvasGroup.DOKill();
        
        await UniTask.WhenAll(
            _workshopView.HideAsync(ct),
            _backdropCanvasGroup.DOFade(0f, _backdropFadeDuration).SetUpdate(true).WithCancellation(ct)
        );

        SetBackdropInteractable(false);
        _levelFlowController.TryStartNextLevel();
    }

    private void HideInstant()
    {
        _backdropCanvasGroup.DOKill();
        _backdropCanvasGroup.alpha = 0f;
        SetBackdropInteractable(false);

        _rewardsView.HideInstant();
        _workshopView.HideInstant();
    }

    private void SetBackdropInteractable(bool active)
    {
        //_backdropCanvasGroup.blocksRaycasts = active;
        _backdropCanvasGroup.interactable = active;
    }
}