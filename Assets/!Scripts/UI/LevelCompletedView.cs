using System;
using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [Header("View Components")]
    [SerializeField] private TokenRewardsView _tokenRewardView;
    
    [Header("UI Elements")]
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private RectTransform _windowRect;
    [SerializeField] private RectTransform _diceSlotRect;
    
    [Header("Animation")]
    [SerializeField] private float _animationDuration = 0.35f;
    [SerializeField] private Ease _ease = Ease.OutBack;
    [Space]
    [SerializeField] private float _buttonFadeDuration = 0.5f;
    
    private CanvasGroup _canvasGroup;
    private float _windowShownY;
    private Vector3 _dicePanelDefaultPos;
    
    #region DI
    private LevelFlowController _levelFlowController;
    private RectTransform _dicePanelRect;
    private DiceStatePanelView _dicePanelView;
    
    [Inject]
    public void Construct(LevelFlowController levelFlowController, DiceStatePanelView dicePanelView)
    {
        _levelFlowController = levelFlowController;
        _dicePanelView = dicePanelView;
        _dicePanelRect = dicePanelView.GetComponent<RectTransform>();
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _windowShownY = _windowRect.anchoredPosition.y;
    }

    private void Start()
    {
        _dicePanelDefaultPos = _dicePanelRect.position;
        HideInstant();
    }

    private float GetHiddenOffsetY()
    {
        float windowHeight = _windowRect.rect.height > 0 ? _windowRect.rect.height : 1200f;
        return -(windowHeight * 1.5f);
    }

    private void OnEnable()
    {
        _levelFlowController.OnLevelCompleted += HandleLevelCompleted;
        _nextLevelButton.onClick.AddListener(HandleNextLevelClicked);
    }

    private void OnDisable()
    {
        _levelFlowController.OnLevelCompleted -= HandleLevelCompleted;
        _nextLevelButton.onClick.RemoveListener(HandleNextLevelClicked);
    }
    
    private void HandleLevelCompleted(TokenRewardData tokenRewardData)
    {
        _tokenRewardView.Setup(tokenRewardData);
        ShowAsync().Forget();
    }

    private void HandleNextLevelClicked() => HideAsync().Forget();
    
    #region Async Methods
    private async UniTaskVoid ShowAsync()
    {
        var ct = destroyCancellationToken;

        ToggleInteractable(true);
        SetWindowHiddenPosition();

        await AnimateShowAsync(ct);

        _dicePanelView.SetRepairMode(true);
    }

    private async UniTaskVoid HideAsync()
    {
        var ct = destroyCancellationToken;

        ToggleInteractable(false);
        _dicePanelView.SetRepairMode(false);

        await UniTask.Delay(TimeSpan.FromSeconds(_buttonFadeDuration), cancellationToken: ct);

        await AnimateHideAsync(ct);

        _levelFlowController.TryStartNextLevel();
    }

    private async UniTask AnimateShowAsync(CancellationToken ct)
    {
        transform.DOKill();
        _dicePanelRect.DOKill();

        _windowRect.anchoredPosition = new Vector2(_windowRect.anchoredPosition.x, _windowShownY);
        Canvas.ForceUpdateCanvases();
        var targetSlotPos = _diceSlotRect.position;
        SetWindowHiddenPosition();
        
        await DOTween.Sequence()
            .SetTarget(transform)
            .SetUpdate(true)
            .Join(_canvasGroup.DOFade(1f, _animationDuration * 0.6f).SetEase(_ease))
            .Join(_windowRect.DOAnchorPosY(_windowShownY, _animationDuration).SetEase(_ease))
            .Join(_dicePanelRect.DOMove(targetSlotPos, _animationDuration).SetEase(_ease))
            .WithCancellation(ct);
    }

    private async UniTask AnimateHideAsync(CancellationToken ct)
    {
        transform.DOKill();
        _dicePanelRect.DOKill();

        var targetHideY = _windowShownY + GetHiddenOffsetY();

        await DOTween.Sequence()
            .SetTarget(transform)
            .SetUpdate(true)
            .Join(_canvasGroup.DOFade(0f, _animationDuration * 0.6f).SetEase(_ease))
            .Join(_windowRect.DOAnchorPosY(targetHideY, _animationDuration).SetEase(_ease))
            .Join(_dicePanelRect.DOMove(_dicePanelDefaultPos, _animationDuration).SetEase(_ease))
            .WithCancellation(ct);
    }
    #endregion

    private void HideInstant()
    {
        _dicePanelView.SetRepairMode(false);
        ToggleInteractable(false);
        SetWindowHiddenPosition();
        _canvasGroup.alpha = 0f;
    }
    
    private void SetWindowHiddenPosition()
    {
        var pos = new Vector2(_windowRect.anchoredPosition.x, _windowShownY + GetHiddenOffsetY());
        _windowRect.anchoredPosition = pos;
    }
    
    private void ToggleInteractable(bool active)
    {
        _nextLevelButton.interactable = active;
        _canvasGroup.blocksRaycasts = active;
        _canvasGroup.interactable = active;
    }
}