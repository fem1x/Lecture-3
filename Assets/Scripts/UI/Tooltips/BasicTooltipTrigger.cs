using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace _Scripts.UI.Tooltips
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class BasicTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private enum Side
        {
            Right,
            Left,
            Above,
            Below
        }

        [SerializeField] private BasicTooltipView _prefab;
        [SerializeField] private RectTransform _targetOverride;
        [SerializeField] private LocalizedString _text = new();
        [SerializeField] private LocalizedString _title = new();
        [SerializeField] private Side _side = Side.Right;
        [SerializeField, Min(0f)] private float _gap = 24f;
        [SerializeField, Min(0f)] private float _hoverDelay = 0f;
        [SerializeField] private bool _showImmediatelyOnFirstHover;

        private RectTransform _target;
        private BasicTooltipView _view;
        private bool _isListening;
        private bool _isHovered;
        private bool _isPinned;
        private string _pendingFirstShowKey;
        private string _currentTitle;
        private bool _hasHovered;
        private bool _hideOnPointerDown;
        private bool _suppressedUntilExit;
        private Coroutine _hoverRoutine;

        public BasicTooltipView Prefab => _prefab;

        public void Configure(BasicTooltipView prefab, LocalizedString text)
        {
            _prefab = prefab;
            _text = text;
            _side = Side.Above;
        }

        public void CopyHoverSettings(BasicTooltipTrigger source)
        {
            _hoverDelay = source._hoverDelay;
            _showImmediatelyOnFirstHover = source._showImmediatelyOnFirstHover;
        }

        public void ConfigureButtonHover() => _hideOnPointerDown = true;

        public Func<string, string> TextFormatter { get; set; }
        public Func<(string title, string text)> ContentProvider { get; set; }

        public void RefreshContent()
        {
            if (!_isListening || ContentProvider == null) return;
            var content = ContentProvider();
            OnTitleChanged(content.title);
            OnTextChanged(content.text);
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        protected virtual void Awake()
        {
            _target = _targetOverride != null ? _targetOverride : (RectTransform)transform;

            // Prefab overrides can change the serialized key after its reference type was deserialized.
            var entry = _text.TableEntryReference;
            entry.OnAfterDeserialize();
            _text.TableEntryReference = entry;
            var titleEntry = _title.TableEntryReference;
            titleEntry.OnAfterDeserialize();
            _title.TableEntryReference = titleEntry;
        }

        protected virtual void LateUpdate()
        {
            if (_view != null && _view.gameObject.activeSelf)
                UpdatePosition();
        }

        protected virtual void OnDisable() => HideImmediately();

        protected virtual void OnDestroy()
        {
            if (_view != null)
            {
                _view.DismissRequested -= DismissAutoTooltip;
                Destroy(_view.gameObject);
            }
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isActiveAndEnabled) return;

            _isHovered = true;
            if (_suppressedUntilExit) return;
            var showImmediately = _showImmediatelyOnFirstHover && !_hasHovered;
            _hasHovered = true;
            if (showImmediately || _hoverDelay <= 0f)
                StartListening();
            else
                _hoverRoutine = StartCoroutine(ShowAfterDelay());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _suppressedUntilExit = false;
            CancelHoverDelay();
            HideIfUnrequested();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_hideOnPointerDown || eventData.button != PointerEventData.InputButton.Left) return;

            DismissButtonHover();
        }

        public void DismissButtonHover()
        {
            _suppressedUntilExit = _isHovered;
            CancelHoverDelay();
            StopListening();
            if (_view != null)
                _view.Hide();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                DismissAutoTooltip();
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_hoverDelay);
            _hoverRoutine = null;
            if (_isHovered && !_suppressedUntilExit)
                StartListening();
        }

        private void CancelHoverDelay()
        {
            if (_hoverRoutine == null) return;
            StopCoroutine(_hoverRoutine);
            _hoverRoutine = null;
        }

        internal void ShowOnFirstVisit()
        {
            if (!isActiveAndEnabled || _text.IsEmpty || _pendingFirstShowKey != null) return;

            var key = $"DiceVice.TooltipSeen.{_text.TableReference.TableCollectionNameGuid:N}.{_text.TableEntryReference.KeyId}";
            if (PlayerPrefs.GetInt(key) != 0) return;

            _pendingFirstShowKey = key;
            if (_isListening && _view != null && _view.gameObject.activeSelf)
                RecordFirstShow();
            else
                StartListening();
        }

        internal void HideImmediately()
        {
            ResetRequests();
            if (_view != null)
                _view.HideImmediately();
        }

        private void DismissAutoTooltip()
        {
            if (!_isPinned && _pendingFirstShowKey == null) return;

            ResetRequests();
            if (_view != null)
                _view.Hide();
        }

        private void ResetRequests()
        {
            CancelHoverDelay();
            _suppressedUntilExit = false;
            _isHovered = false;
            _isPinned = false;
            _pendingFirstShowKey = null;
            StopListening();
        }

        private void HideIfUnrequested()
        {
            if (_isHovered || _isPinned || _pendingFirstShowKey != null) return;

            StopListening();
            if (_view != null)
                _view.Hide();
        }

        private void StartListening()
        {
            if (_isListening) return;

            if (ContentProvider != null)
            {
                _isListening = true;
                var content = ContentProvider();
                OnTitleChanged(content.title);
                OnTextChanged(content.text);
                return;
            }

            if (_text.IsEmpty) return;
            _isListening = true;
            if (!_title.IsEmpty)
                _title.StringChanged += OnTitleChanged;
            _text.StringChanged += OnTextChanged;
        }

        private void OnTextChanged(string text)
        {
            if (!_isListening) return;

            if (string.IsNullOrWhiteSpace(text))
            {
                if (_view != null)
                    _view.HideImmediately();
                return;
            }

            if (_view == null)
            {
                var canvas = GetComponentInParent<Canvas>().rootCanvas;
                _view = Instantiate(_prefab, canvas.transform, false);
                _view.DismissRequested += DismissAutoTooltip;
            }

            _view.SetTitle(_currentTitle);
            _view.SetContent(TextFormatter != null ? TextFormatter(text) : text, GetDirection());
            UpdatePosition();
            _view.Show();

            if (_pendingFirstShowKey != null)
                RecordFirstShow();
            else
                _view.SetDismissible(_isPinned);
        }

        private void OnTitleChanged(string title)
        {
            _currentTitle = title;
            if (_view != null)
                _view.SetTitle(title);
        }

        private void RecordFirstShow()
        {
            PlayerPrefs.SetInt(_pendingFirstShowKey, 1);
            PlayerPrefs.Save();
            _pendingFirstShowKey = null;
            _isPinned = true;
            _view.SetDismissible(true);
        }

        private void StopListening()
        {
            if (!_isListening) return;

            _isListening = false;
            if (ContentProvider != null) return;

            _text.StringChanged -= OnTextChanged;
            if (!_title.IsEmpty)
                _title.StringChanged -= OnTitleChanged;
        }

        private void UpdatePosition()
        {
            var direction = GetDirection();
            var rect = _target.rect;
            var edge = rect.center + Vector2.Scale(rect.size * 0.5f, direction);
            var parent = _view.transform.parent;
            var position = (Vector2)parent.InverseTransformPoint(_target.TransformPoint(edge));
            _view.SetPosition(position + direction * _gap);
        }

        private Vector2 GetDirection() => _side switch
        {
            Side.Left => Vector2.left,
            Side.Above => Vector2.up,
            Side.Below => Vector2.down,
            _ => Vector2.right
        };
    }
}
