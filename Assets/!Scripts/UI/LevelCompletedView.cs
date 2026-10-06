using System;
using _Scripts.Managers;
using _Scripts.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [Header("UI Elements")]
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private RectTransform _windowRect;
    [SerializeField] private RectTransform _diceSlotRect;
    
    [Header("Animation")]
    [SerializeField] private float _animationDuration = 0.35f;
    [SerializeField] private float _hiddenOffsetY = -1000f;
    [SerializeField] private Ease _ease = Ease.OutBack;
    
    private CanvasGroup _canvasGroup;
    private float _windowShownY;
    private Vector3 _dicePanelDefaultPos;
    private Vector3 _dicePanelSlotPos;
    
    #region DI
    private LevelFlowController _levelFlowController;
    private RectTransform _dicePanelRect;
    
    [Inject]
    public void Construct(LevelFlowController levelFlowController, DiceStatePanelView dicePanelView)
    {
        _levelFlowController = levelFlowController;
        _dicePanelRect = dicePanelView.GetComponent<RectTransform>();
    }
    #endregion
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _windowShownY = _windowRect.anchoredPosition.y;
        
        _dicePanelDefaultPos = _dicePanelRect.position;
        _dicePanelSlotPos = _diceSlotRect.position;
        
        HideInstant();
    }

    private void OnEnable()
    {
        _levelFlowController.OnLevelCompleted += Show;
        _nextLevelButton.onClick.AddListener(Hide);
    }

    private void OnDisable()
    {
        _levelFlowController.OnLevelCompleted -= Show;
        _nextLevelButton.onClick.RemoveListener(Hide);
        transform.DOKill();
    }
    
    private void Show()
    {
        ToggleInteractable(true);
        SetWindowHiddenPosition();
        AnimateShow();
    }
    
    private void Hide()
    {
        ToggleInteractable(false);
        AnimateHide();
    }

    private void HideInstant()
    {
        ToggleInteractable(false);
        SetWindowHiddenPosition();
        _canvasGroup.alpha = 0f;
    }
    
    private void Animate(float targetAlpha, float targetWindowY, Vector3 diceTargetPos, Action onComplete = null)
    {
        transform.DOKill();

        DOTween.Sequence()
            .SetTarget(transform)
            .SetUpdate(true)
            .Join(_canvasGroup.DOFade(targetAlpha, _animationDuration * 0.6f).SetEase(_ease))
            .Join(_windowRect.DOAnchorPosY(targetWindowY, _animationDuration).SetEase(_ease))
            .Join(_dicePanelRect.DOMove(diceTargetPos, _animationDuration).SetEase(_ease))
            .OnComplete(() => onComplete?.Invoke());
    }
    
    private void SetWindowHiddenPosition()
    {
        var pos = new Vector2(_windowRect.anchoredPosition.x, _windowShownY + _hiddenOffsetY);
        _windowRect.anchoredPosition = pos;
    }
    
    private void ToggleInteractable(bool active)
    {
        _nextLevelButton.interactable = active;
        _canvasGroup.blocksRaycasts = active;
        _canvasGroup.interactable = active;
    }
    
    private void AnimateShow() => Animate(1f, _windowShownY, _dicePanelSlotPos);
    private void AnimateHide() => Animate(0f, _windowShownY + _hiddenOffsetY, _dicePanelDefaultPos, 
        () => _levelFlowController.TryStartNextLevel());
}
