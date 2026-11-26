using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

namespace _Scripts.UI.Tooltips
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class BasicTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
        [SerializeField] private Side _side = Side.Right;
        [SerializeField, Min(0f)] private float _gap = 24f;

        private RectTransform _target;
        private BasicTooltipView _view;
        private bool _isListening;

        public Func<string, string> TextFormatter { get; set; }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _target = (RectTransform)transform;

            // Prefab overrides can change the serialized key after its reference type was deserialized.
            var entry = _text.TableEntryReference;
            entry.OnAfterDeserialize();
            _text.TableEntryReference = entry;
        }

        private void LateUpdate()
        {
            if (_view != null && _view.gameObject.activeSelf)
                UpdatePosition();
        }

        private void OnDisable()
        {
            StopListening();
            if (_view != null)
                _view.HideImmediately();
        }

        private void OnDestroy()
        {
            if (_view != null)
                Destroy(_view.gameObject);
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isListening || _text.IsEmpty) return;

            _isListening = true;
            _text.StringChanged += OnTextChanged;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopListening();
            if (_view != null)
                _view.Hide();
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
            }

            _view.SetContent(TextFormatter != null ? TextFormatter(text) : text, GetDirection());
            UpdatePosition();
            _view.Show();
        }

        private void StopListening()
        {
            if (!_isListening) return;

            _isListening = false;
            _text.StringChanged -= OnTextChanged;
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
