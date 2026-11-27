using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI.Charms
{
    public class CharmSlotView : MonoBehaviour, IDropHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RectTransform _slotRect;
        [SerializeField] private CharmEquippedView _charmPrefab;
        [SerializeField] private Transform _charmHolder;
        [SerializeField] private GameObject _emptyPlaceholder;

        [Header("Empty Slot Highlight")]
        [SerializeField] private Color _highlightColor = new(0.65f, 0.55f, 0.72f, 1f);
        [SerializeField] private float _highlightScale = 1.28f;
        [SerializeField] private float _highlightTransitionTime = 0.22f;
        [SerializeField] private float _highlightPulseTime = 0.8f;
        [SerializeField] private float _highlightPulseRiseTime = 0.12f;
        [SerializeField] private float _highlightStaggerDelay = 0.12f;
        public bool IsEmpty => _currentEquipped == null;

        private CharmEquippedView _currentEquipped;
        private CharmBase _activeCharm;
        private Image _placeholderImage;
        private Color _placeholderColor;
        private Vector3 _placeholderScale;
        private Tween _highlightTween;
        
        #region DI
        private CharmReplaceView _replaceView;
        private CharmsConveyorService _conveyorService;
        private IObjectResolver _resolver;
        private CharmDragState _dragState;
        private ISfxPlayer _sfxPlayer;
        private CharmsService _charmsService;
        
        [Inject]
        public void Construct(
            CharmsConveyorService conveyorService, 
            IObjectResolver resolver, 
            CharmReplaceView replaceView,
            CharmDragState dragState,
            ISfxPlayer sfxPlayer,
            CharmsService charmsService)
        {
            _conveyorService = conveyorService;
            _resolver = resolver;
            _replaceView = replaceView;
            _dragState = dragState;
            _sfxPlayer = sfxPlayer;
            _charmsService = charmsService;
            _dragState.OnDragChanged += HandleDragChanged;
            if (isActiveAndEnabled && _dragState.IsDragging)
                HandleDragChanged(true);
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnEnable()
        {
            if (_dragState != null && _dragState.IsDragging)
                HandleDragChanged(true);
        }

        private void OnDisable() => ResetHighlight();

        private void OnDestroy()
        {
            if (_dragState != null)
                _dragState.OnDragChanged -= HandleDragChanged;
            _highlightTween?.Kill();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void InitializePlaceholder()
        {
            if (_placeholderImage != null) return;

            _placeholderImage = _emptyPlaceholder.GetComponent<Image>();
            _placeholderColor = _placeholderImage.color;
            _placeholderScale = _placeholderImage.transform.localScale;
        }

        private void HandleDragChanged(bool isDragging)
        {
            if (!isActiveAndEnabled || !IsEmpty) return;

            InitializePlaceholder();
            _highlightTween?.Kill();
            var targetColor = isDragging ? Color.Lerp(_placeholderColor, _highlightColor, 0.55f) : _placeholderColor;
            var targetScale = _placeholderScale;
            _highlightTween = DOTween.Sequence()
                .Join(_placeholderImage.DOColor(targetColor, _highlightTransitionTime))
                .Join(_placeholderImage.transform.DOScale(targetScale, _highlightTransitionTime))
                .SetEase(Ease.OutSine)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    if (isDragging) ScheduleHighlightPulse();
                });
        }

        private void ScheduleHighlightPulse()
        {
            _highlightTween = DOTween.Sequence()
                .AppendInterval(_highlightStaggerDelay * GetHighlightWaveIndex())
                .AppendCallback(StartHighlightPulse)
                .SetLink(gameObject);
        }

        private int GetHighlightWaveIndex()
        {
            var parent = transform.parent;
            if (parent == null) return 0;

            var position = transform.localPosition;
            var index = 0;
            foreach (Transform sibling in parent)
            {
                if (sibling == transform || !sibling.TryGetComponent<CharmSlotView>(out _)) continue;

                var siblingPosition = sibling.localPosition;
                if (siblingPosition.y > position.y ||
                    (siblingPosition.y == position.y && siblingPosition.x < position.x) ||
                    (siblingPosition.y == position.y && siblingPosition.x == position.x &&
                     sibling.GetSiblingIndex() < transform.GetSiblingIndex()))
                    index++;
            }

            return index;
        }

        private void StartHighlightPulse()
        {
            var restingColor = Color.Lerp(_placeholderColor, _highlightColor, 0.55f);
            _highlightTween = DOTween.Sequence()
                .Append(_placeholderImage.transform.DOScale(_placeholderScale * _highlightScale, _highlightPulseRiseTime)
                    .SetEase(Ease.OutQuad))
                .Join(_placeholderImage.DOColor(_highlightColor, _highlightPulseRiseTime).SetEase(Ease.OutQuad))
                .Append(_placeholderImage.transform.DOScale(_placeholderScale, _highlightPulseTime)
                    .SetEase(Ease.OutCubic))
                .Join(_placeholderImage.DOColor(restingColor, _highlightPulseTime).SetEase(Ease.OutCubic))
                .SetLoops(-1, LoopType.Restart)
                .SetLink(gameObject);
        }

        private void ResetHighlight()
        {
            _highlightTween?.Kill();
            _highlightTween = null;
            if (_placeholderImage == null) return;

            _placeholderImage.color = _placeholderColor;
            _placeholderImage.transform.localScale = _placeholderScale;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void ClearForNewGame()
        {
            if (_activeCharm == null) return;

            _charmsService.RemoveCharm(_activeCharm);
            Clear();
        }

        public void EquipInitialCharm(CharmConfigBase config)
        {
            if (_charmsService.TryAddCharm(config, out var charm))
                Equip(charm, false);
            else
                Debug.LogWarning($"Could not equip initial charm {config.name}: all charm slots are full.", this);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;
            if (!eventData.pointerDrag.TryGetComponent<CharmDragView>(out var dragView)) return;
            if (!dragView.IsDraggedBy(eventData.pointerId)) return;
            
            if (IsEmpty)
            {
                TryEquip(dragView);
                return;
            }

            dragView.StartPending();
            _replaceView.Show(
                _slotRect, 
                _currentEquipped.CharmGeneralView, 
                dragView.GeneralView,
                onConfirm: () => Replace(dragView),
                onCancel: () => dragView.StopPending()
            );
        }

        private void TryEquip(CharmDragView dragView)
        {
            if (!_conveyorService.TryClaim(dragView.GeneralView.Config, out var createdCharm)) return;

            Equip(createdCharm);
            dragView.Consume();
        }

        private void Equip(CharmBase charm, bool playSound = true)
        {
            ResetHighlight();
            _emptyPlaceholder.SetActive(false);
            _activeCharm = charm;

            _currentEquipped = _resolver.Instantiate(_charmPrefab, _charmHolder);
            _currentEquipped.CharmGeneralView.Setup(charm.Config);
            _currentEquipped.PlayTriggerAnimation();

            charm.AttachView(_currentEquipped);
            if (playSound)
                _sfxPlayer.PlayUI("CharmEquip", _slotRect);
        }
        
        private void Clear()
        {
            if (_currentEquipped != null)
            {
                _activeCharm?.DetachView();
                Destroy(_currentEquipped.gameObject);
                _currentEquipped = null;
                _activeCharm = null;
            }

            _emptyPlaceholder.SetActive(true);
            if (_dragState.IsDragging) HandleDragChanged(true);
        }

        private void Replace(CharmDragView dragView)
        {
            var oldCharm = _activeCharm;
            Clear();

            var newCharm = _conveyorService.Replace(oldCharm, dragView.GeneralView.Config);
            Equip(newCharm);
    
            dragView.Consume();
        }
    }
}
