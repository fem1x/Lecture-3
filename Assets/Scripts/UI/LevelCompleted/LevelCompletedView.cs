using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.UI;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [SerializeField] private CanvasGroup _cg;
    
    [Header("Panels")]
    [SerializeField] private LevelRewardsView _rewardsView;
    [SerializeField] private WorkshopPanelView _workshopView;

    [Header("Backdrop")]
    [SerializeField] private BackdropView _backdropView;

    #region DI
    private LevelFlowController _levelFlowController;
    private MapService _mapService;

    [Inject]
    public void Construct(LevelFlowController levelFlowController, MapService mapService)
    {
        _levelFlowController = levelFlowController;
        _mapService = mapService;
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Awake() => HideInstant();

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
        _cg.Show();
        var ct = destroyCancellationToken;

        await UniTask.WhenAll(
            _backdropView.ShowAsync(),
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
        await _workshopView.HideAsync(ct);
        _mapService.RequestMap();
    }

    private void HideInstant()
    {
        _backdropView.HideInstant();
        _rewardsView.HideInstant();
        _workshopView.HideInstant();
    }
}