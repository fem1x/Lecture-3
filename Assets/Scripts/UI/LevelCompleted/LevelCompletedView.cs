using System.Threading;
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
    [SerializeField] private RewardsWorkshopTransition _workshopTransition;

    [Header("Backdrop")]
    [SerializeField] private BackdropView _backdropView;

    #region DI
    private LevelFlowController _levelFlowController;
    private MapService _mapService;
    private ISfxPlayer _sfxPlayer;

    [Inject]
    public void Construct(LevelFlowController levelFlowController, MapService mapService, ISfxPlayer sfxPlayer)
    {
        _levelFlowController = levelFlowController;
        _mapService = mapService;
        _sfxPlayer = sfxPlayer;
    }
    #endregion

    private CancellationTokenSource _workshopOpening;
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
        _workshopOpening?.Cancel();
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
        if (_workshopOpening != null) return;

        using var opening = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        _workshopOpening = opening;
        _rewardsView.BeginWorkshopTransition();
        SetPanelInteraction(false);

        try
        {
            _workshopView.PrepareOpening();
            await UniTask.WhenAll(
                MorphIntoWorkshopAsync(opening.Token),
                _workshopView.RevealContentsAsync(_workshopTransition.Duration, opening.Token));
            opening.Token.ThrowIfCancellationRequested();
            _workshopView.ShowOpeningTooltips();
        }
        catch
        {
            opening.Cancel();
            if (_workshopView)
                _workshopView.CancelOpening();
            if (_rewardsView)
                _rewardsView.HideInstant();
            if (_backdropView)
                _backdropView.HideInstant();
            if (_cg)
                _cg.Hide();
            throw;
        }
        finally
        {
            _workshopOpening = null;
        }
    }

    private async UniTask MorphIntoWorkshopAsync(CancellationToken ct)
    {
        await _workshopTransition.PlayAsync(_rewardsView.WorkshopButtonRect, _workshopView.FirstMainSlide, _sfxPlayer, ct);
        _rewardsView.HideInstant();
        SetPanelInteraction(true);
    }

    private void SetPanelInteraction(bool enabled)
    {
        _cg.interactable = enabled;
        _cg.blocksRaycasts = enabled;
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
