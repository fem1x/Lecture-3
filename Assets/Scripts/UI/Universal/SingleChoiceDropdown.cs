using System.Collections.Generic;
using _Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public sealed class SingleChoiceDropdown : TMP_Dropdown
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private string _hoverSfx = "ButtonHover";
        [SerializeField] private string _clickSfx = "ButtonClick";
        [SerializeField] private string _disabledClickSfx = "ButtonDisabledClick";
        [SerializeField] private ButtonScaleView _headerFeedback;
        [SerializeField] private RectTransform _arrow;
        [SerializeField, Min(0f)] private float _arrowTime = 0.16f;
        [SerializeField, Range(0.05f, 1f)] private float _collapsedHeight = 0.2f;
        [SerializeField] private Ease _openEase = Ease.OutBack;
        [SerializeField] private Ease _closeEase = Ease.InCubic;

        private readonly List<Toggle> _items = new();
        private ISfxPlayer _sfxPlayer;
        private RectTransform _popup;
        private Vector3 _expandedScale;
        private Tween _popupTween;
        private Tween _arrowTween;
        private Quaternion _closedArrowRotation;
        private bool _restoreHeaderFeedback;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        protected override void Awake()
        {
            base.Awake();
            _closedArrowRotation = _arrow.localRotation;
            onValueChanged.AddListener(UpdateCheckmarks);
        }

        protected override void OnDestroy()
        {
            onValueChanged.RemoveListener(UpdateCheckmarks);
            base.OnDestroy();
        }

        protected override GameObject CreateDropdownList(GameObject template)
        {
            _restoreHeaderFeedback = _headerFeedback.enabled;
            _headerFeedback.enabled = false;
            _items.Clear();
            var list = base.CreateDropdownList(template);
            _popup = (RectTransform)list.transform;
            _expandedScale = _popup.localScale;
            return list;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsInteractable() && !IsExpanded)
                _sfxPlayer.Play(_hoverSfx);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (eventData.button == PointerEventData.InputButton.Left && !IsInteractable())
                _sfxPlayer.Play(_disabledClickSfx);
        }

        protected override GameObject CreateBlocker(Canvas rootCanvas)
        {
            _sfxPlayer.Play(_clickSfx);
            TurnArrow(true);
            _popup.localScale = CollapsedScale();
            _popupTween = _popup.DOScale(_expandedScale, alphaFadeSpeed).SetEase(_openEase).SetUpdate(true);
            return base.CreateBlocker(rootCanvas);
        }

        protected override void DestroyBlocker(GameObject blocker)
        {
            if (_popup != null)
            {
                TurnArrow(false);
                _popupTween?.Kill();
                var group = _popup.GetComponent<CanvasGroup>();
                group.interactable = false;
                group.blocksRaycasts = false;
                _popupTween = _popup.DOScale(CollapsedScale(), alphaFadeSpeed).SetEase(_closeEase).SetUpdate(true);
            }
            base.DestroyBlocker(blocker);
        }

        protected override DropdownItem CreateItem(DropdownItem itemTemplate)
        {
            var item = base.CreateItem(itemTemplate);
            item.toggle.toggleTransition = Toggle.ToggleTransition.None;
            item.gameObject.AddComponent<DropdownOptionSfxView>()
                .Initialize(item.toggle, _sfxPlayer, _hoverSfx, _clickSfx);
            _items.Add(item.toggle);
            return item;
        }

        protected override void DestroyDropdownList(GameObject dropdownList)
        {
            _items.Clear();
            _popupTween?.Kill();
            _popup = null;
            if (_restoreHeaderFeedback)
                _headerFeedback.enabled = true;
            _restoreHeaderFeedback = false;
            base.DestroyDropdownList(dropdownList);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _arrowTween?.Kill();
            _arrow.localRotation = _closedArrowRotation;
        }

        private void TurnArrow(bool expanded)
        {
            _arrowTween?.Kill();
            var rotation = expanded ? _closedArrowRotation * Quaternion.Euler(0f, 0f, 180f) : _closedArrowRotation;
            _arrowTween = _arrow.DOLocalRotateQuaternion(rotation, _arrowTime)
                .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        private Vector3 CollapsedScale() =>
            new(_expandedScale.x * 0.96f, _expandedScale.y * _collapsedHeight, _expandedScale.z);

        private void UpdateCheckmarks(int selectedIndex)
        {
            if (MultiSelect) return;

            for (var i = 0; i < _items.Count; i++)
                _items[i].SetIsOnWithoutNotify(i == selectedIndex);
        }
    }
}
