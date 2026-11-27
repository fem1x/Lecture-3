using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace _Scripts.UI.Tooltips
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BasicTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        [SerializeField] private LocalizedString _text = new();
        [SerializeField] private LocalizedString _title = new();
        [SerializeField] private Side _side = Side.Right;
        [SerializeField, Min(0f)] private float _gap = 24f;

        private RectTransform _target;
        private BasicTooltipView _view;
        private bool _isListening;
        private bool _isHovered;
        private bool _isPinned;
        private string _pendingFirstShowKey;
        private string _currentTitle;

        public Func<string, string> TextFormatter { get; set; }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _target = (RectTransform)transform;

            // Prefab overrides can change the serialized key after its reference type was deserialized.
            var entry = _text.TableEntryReference;
            entry.OnAfterDeserialize();
            _text.TableEntryReference = entry;
            var titleEntry = _title.TableEntryReference;
            titleEntry.OnAfterDeserialize();
            _title.TableEntryReference = titleEntry;
        }

        private void LateUpdate()
        {
            if (_view != null && _view.gameObject.activeSelf)
                UpdatePosition();
        }

        private void OnDisable() => HideImmediately();

        private void OnDestroy()
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
            StartListening();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            HideIfUnrequested();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                DismissAutoTooltip();
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
            if (_isListening || _text.IsEmpty) return;

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
