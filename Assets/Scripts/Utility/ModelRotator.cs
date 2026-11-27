using System;
using UnityEngine;

namespace _Scripts.Utility
{
    public class ModelRotator : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action InteractionStarted;

        [SerializeField] private float _sensitivity = 0.25f;
        [SerializeField] private float _damping = 8f;
        [SerializeField] private Camera _camera;

        private Vector2 _angularVelocity;
        private bool _isDragging;
        private bool _directPointerDrag;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        internal void ConfigurePointerDrag(Camera camera, float degreesPerPixel, float damping)
        {
            _camera = camera;
            _sensitivity = degreesPerPixel;
            _damping = damping;
            _directPointerDrag = true;
        }

        public void ResetVelocity() => _angularVelocity = Vector2.zero;

        public void BeginDrag()
        {
            _isDragging = true;
            _angularVelocity = Vector2.zero;
            InteractionStarted?.Invoke();
        }

        public void Drag(Vector2 screenDelta)
        {
            if (!_isDragging) return;
            if (_directPointerDrag)
            {
                var rotation = screenDelta * _sensitivity;
                ApplyRotation(rotation);
                var velocity = rotation / Mathf.Max(Time.unscaledDeltaTime, 0.008f);
                _angularVelocity = Vector2.ClampMagnitude(velocity, 480f);
                return;
            }

            _angularVelocity = new(screenDelta.x * _sensitivity, screenDelta.y * _sensitivity);
            ApplyRotation(_angularVelocity * Time.deltaTime);
        }

        public void EndDrag() => _isDragging = false;

        private void Update()
        {
            if (_isDragging) return;

            if (_directPointerDrag)
            {
                var decay = 1f - Mathf.Exp(-_damping * Time.unscaledDeltaTime);
                if (_angularVelocity.sqrMagnitude > 0.01f)
                    ApplyRotation(_angularVelocity * (decay / _damping));
                _angularVelocity *= 1f - decay;
                return;
            }

            _angularVelocity = Vector2.Lerp(_angularVelocity, Vector2.zero, _damping * Time.deltaTime);

            if (_angularVelocity.sqrMagnitude > 0.0001f)
                ApplyRotation(_angularVelocity * Time.deltaTime);
        }

        private void ApplyRotation(Vector2 rotationDelta)
        {
            if (_camera == null) return;

            var camTransform = _camera.transform;
            var rotationY = Quaternion.AngleAxis(-rotationDelta.x, camTransform.up);
            var rotationX = Quaternion.AngleAxis(rotationDelta.y, camTransform.right);

            transform.rotation = rotationY * rotationX * transform.rotation;
        }
    }
}
