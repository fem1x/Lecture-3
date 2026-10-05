using _Scripts.Managers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

[RequireComponent(typeof(CanvasGroup))]
public class LevelCompletedView : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private float _fadeDuration = 0.3f;
    
    private CanvasGroup _canvasGroup;
    private LevelFlowController _levelFlowController;

    [Inject]
    public void Construct(LevelFlowController levelFlowController)
    {
        _levelFlowController = levelFlowController;
    }
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        HideInstant();
    }

    private void OnEnable()
    {
        _levelFlowController.OnLevelCompleted += Show;
        _nextLevelButton.onClick.AddListener(OnNextLevelClicked);
    }

    private void OnDisable()
    {
        _levelFlowController.OnLevelCompleted -= Show;
        _nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
        _canvasGroup.DOKill();
    }

    private void OnNextLevelClicked()
    {
        _nextLevelButton.interactable = false;
        
        _canvasGroup.DOKill();
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        _canvasGroup.DOFade(0f, _fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _levelFlowController.TryStartNextLevel();
            });
    }

    private void Show()
    {
        _canvasGroup.DOKill();
        _nextLevelButton.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;
        _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);
    }

    private void HideInstant()
    {
        _canvasGroup.DOKill();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }
}
