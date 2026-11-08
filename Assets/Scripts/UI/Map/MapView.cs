using System.Collections.Generic;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI
{
    public class MapView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("References")] 
        [SerializeField] private SlidingPanelView _slidingPanelView;
        [SerializeField] private BackdropView _backdrop;
        [Space]
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private RectTransform _containerRect;
        [SerializeField] private MapSectionView _sectionPrefab;
        [SerializeField] private TMP_Text _sectionText;
        
        [Header("Buttons")]
        [SerializeField] private Button _upButton;
        [SerializeField] private Button _downButton;
        [SerializeField] private Button _startButton;
        
        [Header("Movement")]
        [SerializeField] private float _moveTime = 0.8f;
        [SerializeField] private Ease  _moveEase = Ease.OutCubic;
        
        private Tween _moveTween;
        private int _sectionsCount;
        private readonly List<RectTransform> _sectionsList = new();
        
        private MapService _mapService;
        private IObjectResolver _resolver;
        private LevelFlowController _levelFlowController;

        [Inject]
        public void Construct(MapService mapService,  IObjectResolver resolver, LevelFlowController levelFlowController)
        {
            _mapService = mapService;
            _resolver = resolver;
            _levelFlowController = levelFlowController;
            
            _mapService.OnMapInitialized += Initialize;
            _mapService.OnSectionChanged += MoveToSection;
            _mapService.OnMapRequested += Show;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnDestroy()
        {
            _mapService.OnMapInitialized -= Initialize;
            _mapService.OnSectionChanged -= MoveToSection;
            _mapService.OnMapRequested -= Show;

        }
        
        private void OnEnable()
        {
            _upButton.onClick.AddListener(_mapService.MoveUp);
            _downButton.onClick.AddListener(_mapService.MoveDown);
            _startButton.onClick.AddListener(HandleStartButtonPressed);
        }

        private void OnDisable()
        {
            _upButton.onClick.RemoveListener(_mapService.MoveUp);
            _downButton.onClick.RemoveListener(_mapService.MoveDown);
            _startButton.onClick.RemoveListener(HandleStartButtonPressed);
        }

        private void Initialize()
        {
            foreach (var section in _mapService.SectionsList)
            {
                var spawnedSection = _resolver.Instantiate(_sectionPrefab, _containerRect);
                spawnedSection.Setup(section);
                _sectionsList.Add(spawnedSection.transform as RectTransform);
            }

            _sectionsCount = _mapService.SectionsList.Count;
            AdjustContainerLayout();
            MoveToSectionInstant(_mapService.SelectedSection);
        }
        
        private void MoveToSection(int index)
        {
            _moveTween?.Kill();
            var targetY = -_sectionsList[index - 1].transform.localPosition.y;
            _moveTween = _containerRect.DOAnchorPosY(targetY, _moveTime).SetEase(_moveEase);
            
            RefreshText();
            RefreshButtonsState();
        }

        private void MoveToSectionInstant(int index)
        {
            _moveTween?.Kill();
            var targetY = -_sectionsList[index - 1].transform.localPosition.y;
            _containerRect.anchoredPosition = new Vector2(_containerRect.anchoredPosition.x, targetY);

            RefreshText();
            RefreshButtonsState();
        }
        
        private void RefreshButtonsState()
        {
            _upButton.interactable = _mapService.CanMoveUp;
            _downButton.interactable = _mapService.CanMoveDown;
        }

        private void RefreshText()
        {
            _sectionText.text = $"{_mapService.SelectedSection} / {_sectionsCount}";
        }
        
        private void AdjustContainerLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_containerRect);
            Canvas.ForceUpdateCanvases();
        }

        private void HandleStartButtonPressed()
        {
            DoStartAsync().Forget();
        }

        private async UniTaskVoid DoStartAsync()
        {
            await HideAsync();
            _levelFlowController.StartLevel();
        }

        private void Show(int sectionIndex)
        {
            MoveToSectionInstant(sectionIndex);
            _slidingPanelView.ShowAsync().Forget();
            _backdrop.ShowAsync().Forget();
        }

        private async UniTask HideAsync()
        {
            await UniTask.WhenAll(
                _slidingPanelView.HideAsync(), 
                _backdrop.HideAsync());
        }
    }
}