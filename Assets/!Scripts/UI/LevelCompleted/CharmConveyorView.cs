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
        [SerializeField] private CharmView _charmPrefab;
        [SerializeField] private HorizontalLayoutGroup _containerLayout;
        
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

        private RectTransform _containerRect;
        private readonly List<CharmView> _spawnedViewsList = new();
        private Tween _moveTween;
        
        #region DI
        private CharmsConveyorService  _conveyorService;
        private IObjectResolver _resolver;

        [Inject]
        public void Construct(CharmsConveyorService conveyorService, IObjectResolver resolver)
        {
            _resolver = resolver;
            _conveyorService = conveyorService;
            
            _conveyorService.OnStateChanged += RefreshButtonsState;
            _conveyorService.OnConveyorMoved += MoveToCharm;
            
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
            _moveTween?.Kill();
        }
        
        private void HandleNextButtonClick() => _conveyorService.MoveNext();
        private void HandleReverseButtonClick() => _conveyorService.MoveReverse();
        
        private void RefreshButtonsState()
        {
            _nextButton.interactable = _conveyorService.CanMoveNext;
            _reverseButton.interactable = _conveyorService.CanMoveReverse;
        }
        
        public void InitializeCharmViews()
        {
            ClearCharmViews();
            
            foreach (var charm in _conveyorService.Charms)
            {
                var view = _resolver.Instantiate(_charmPrefab, _containerRect);
                view.Setup(charm);
                _spawnedViewsList.Add(view);
            }

            AdjustContainerLayout();
            ResetConveyorPosition();
        }

        private void MoveToCharm(int charmIndex)
        {
            _moveTween?.Kill();
            var targetX = -_spawnedViewsList[charmIndex].transform.localPosition.x;
            _moveTween = _containerRect.DOAnchorPosX(targetX, _moveDuration).SetEase(_moveEase);
            
            UpdateIndicators(charmIndex);
        }

        private void UpdateIndicators(int charmIndex)
        {
            for (int i = 0; i < _indicators.Count; i++)
                _indicators[i].color = charmIndex == i ? _activeColor : _inactiveColor;
        }
        
        private void ClearCharmViews()
        {
            foreach (var view in _spawnedViewsList)
                Destroy(view.gameObject);
            _spawnedViewsList.Clear();
        }

        private void ResetConveyorPosition()
        {
            _moveTween?.Kill();
            _containerRect.anchoredPosition = new Vector2(-_spawnedViewsList[0].transform.localPosition.x, _containerRect.anchoredPosition.y);
        }

        private void AdjustContainerLayout()
        {
            var cardWidth = ((RectTransform)_spawnedViewsList[0].transform).rect.width;
            var conveyorWidth = ((RectTransform)_containerRect.parent).rect.width;

            _containerLayout.childForceExpandWidth = false;
            _containerLayout.spacing = (conveyorWidth / 2f) - cardWidth;

            _containerRect.offsetMin = new Vector2(-cardWidth / 2f, _containerRect.offsetMin.y);
            _containerRect.offsetMax = new Vector2(cardWidth / 2f, _containerRect.offsetMax.y);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_containerRect);
        }
    }
}