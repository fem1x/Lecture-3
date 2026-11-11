using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Utility
{
    [RequireComponent(typeof(Collider))]
    public class ModelRotator : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private float _sensitivity = 0.25f;
        [SerializeField] private float _damping = 8f;
        [SerializeField] private Camera _camera;

        private Vector2 _angularVelocity;
        private Vector2 _lastMousePosition;
        private bool _isDragging;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnMouseDown()
        {
            _isDragging = true;
            _lastMousePosition = Mouse.current.position.ReadValue();
            _angularVelocity = Vector2.zero;
        }

        private void OnMouseUp() => _isDragging = false;

        private void Update()
        {
            if (Mouse.current == null) return;

            if (_isDragging)
            {
                if (!Mouse.current.leftButton.isPressed)
                {
                    _isDragging = false;
                    return;
                }

                Vector2 currentMousePosition = Mouse.current.position.ReadValue();
                var delta = currentMousePosition - _lastMousePosition;
                _lastMousePosition = currentMousePosition;

                _angularVelocity = new(delta.x * _sensitivity, delta.y * _sensitivity);
            }
            else
            {
                _angularVelocity = Vector2.Lerp(_angularVelocity, Vector2.zero, _damping * Time.deltaTime);
            }

            if (_angularVelocity.sqrMagnitude > 0.0001f)
                ApplyRotation(_angularVelocity * Time.deltaTime);
        }

        private void ApplyRotation(Vector2 rotationDelta)
        {
            var camTransform = _camera.transform;
            var rotationY = Quaternion.AngleAxis(-rotationDelta.x, camTransform.up);
            var rotationX = Quaternion.AngleAxis(rotationDelta.y, camTransform.right);

            transform.rotation = rotationY * rotationX * transform.rotation;
        }
    }
}