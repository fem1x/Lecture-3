using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.UI.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI
{
    public class CharmConveyorView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Charms Spawn")]
        [SerializeField] private CharmDragView _charmPrefab;
        [SerializeField] private ConveyorCellView _cellPrefab;
        [SerializeField] private HorizontalLayoutGroup _containerLayout;
        [SerializeField] private RectTransform _dragLayer;
        
        [Header("Conveyor Movement")]
        [SerializeField] private float _moveDuration = 0.8f;
        [SerializeField] private Ease  _moveEase = Ease.OutCubic;
        
        [Header("Buttons")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _reverseButton;
        
        [Header("Index Indicators")]
        [SerializeField] private List<Image> _indicators;
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _inactiveColor;
        
        [Header("Shutter")]
        [SerializeField] private ConveyorShutterView _shutter;

        private readonly List<ConveyorCellView> _spawnedCellsList = new();
        private RectTransform _containerRect;
        private Tween _moveTween;
        
        #region DI
        private CharmsConveyorService  _conveyorService;
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(CharmsConveyorService conveyorService, IObjectResolver resolver)
        {
            _resolver = resolver;
            _conveyorService = conveyorService;

            _conveyorService.OnConveyorInitialized += InitializeConveyorView;
            _conveyorService.OnStateChanged += RefreshButtonsState;
            _conveyorService.OnConveyorMoved += MoveToCharm;
            _conveyorService.OnChoiceCompleted += CloseConveyor;

            RefreshButtonsState();
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _containerRect = (RectTransform)_containerLayout.transform;
        
        private void OnEnable()
        {
            _nextButton.onClick.AddListener(HandleNextButtonClick);
            _reverseButton.onClick.AddListener(HandleReverseButtonClick);
        }

        private void OnDisable()
        {
            _nextButton.onClick.RemoveListener(HandleNextButtonClick);
            _reverseButton.onClick.RemoveListener(HandleReverseButtonClick);
        }

        private void OnDestroy()
        {
            _conveyorService.OnStateChanged -= RefreshButtonsState;
            _conveyorService.OnConveyorMoved -= MoveToCharm;
            _conveyorService.OnConveyorInitialized -= InitializeConveyorView;
            _conveyorService.OnChoiceCompleted -= CloseConveyor;
            _moveTween?.Kill();
        }
        
        private void HandleNextButtonClick() => _conveyorService.MoveNext();
        private void HandleReverseButtonClick() => _conveyorService.MoveReverse();
        
        private void RefreshButtonsState()
        {
            _nextButton.interactable = _conveyorService.CanMoveNext;
            _reverseButton.interactable = _conveyorService.CanMoveReverse;
        }
        
        public void InitializeConveyorView()
        {
            ClearCharmViews();
            _containerRect.anchoredPosition = Vector2.zero;
    
            foreach (var charm in _conveyorService.Charms)
            {
                var cell = _resolver.Instantiate(_cellPrefab, _containerRect);
                var card = _resolver.Instantiate(_charmPrefab, cell.CharmHolder);
                card.Setup(charm, _dragLayer, cell.CharmHolder);
                cell.AttachCharm(card);

                _spawnedCellsList.Add(cell);
            }

            AdjustContainerLayout();
            ResetConveyorPosition();
            UpdateDraggableCharm(0);
            UpdateIndicators(0);
            _shutter.ReOpen();
        }

        private void MoveToCharm(int charmIndex)
        {
            ResetCharmStates();
            
            _moveTween?.Kill();
            var targetX = -_spawnedCellsList[charmIndex].transform.localPosition.x;
            _moveTween = _containerRect.DOAnchorPosX(targetX, _moveDuration).SetEase(_moveEase);
    
            UpdateIndicators(charmIndex);
            UpdateDraggableCharm(charmIndex);
        }

        private void UpdateIndicators(int activeIndex)
        {
            for (int i = 0; i < _indicators.Count; i++)
                _indicators[i].color = activeIndex == i ? _activeColor : _inactiveColor;
        }
        
        private void UpdateDraggableCharm(int activeIndex)
        {
            for (var i = 0; i < _spawnedCellsList.Count; i++)
                _spawnedCellsList[i].SetDraggable(i == activeIndex);
        }
        
        private void ClearCharmViews()
        {
            foreach (var cell in _spawnedCellsList)
            {
                cell.transform.SetParent(null);
                Destroy(cell.gameObject);
            }
        
            _spawnedCellsList.Clear();
        }

        private void ResetConveyorPosition()
        {
            _moveTween?.Kill();
            var targetX = -_spawnedCellsList[0].transform.localPosition.x;
            _containerRect.anchoredPosition = new Vector2(targetX, _containerRect.anchoredPosition.y);
        }

        private void AdjustContainerLayout()
        {
            var cardWidth = ((RectTransform)_spawnedCellsList[0].transform).rect.width;
            var conveyorWidth = ((RectTransform)_containerRect.parent).rect.width;

            _containerLayout.childForceExpandWidth = false;
            _containerLayout.spacing = (conveyorWidth / 2f) - cardWidth;

            _containerRect.offsetMin = new Vector2(-cardWidth / 2f, _containerRect.offsetMin.y);
            _containerRect.offsetMax = new Vector2(cardWidth / 2f, _containerRect.offsetMax.y);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_containerRect);
            Canvas.ForceUpdateCanvases();
        }

        private void ResetCharmStates()
        {
            foreach (var cell in _spawnedCellsList)
            {
                var charm = cell.CurrentCharm;
                if (charm != null)
                    charm.InstantReturnToConveyor();
            }
        }

        private void CloseConveyor()
        {
            _nextButton.interactable = false;
            _reverseButton.interactable = false;
            UpdateDraggableCharm(-1);
            
            _shutter.Close();
        }
    }
}