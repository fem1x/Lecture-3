using System;
using _Scripts.Configs;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Managers
{
    public class CameraShaker : IDisposable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly Transform _cameraTransform;
        private readonly Vector3 _originalLocalPos;
        private readonly Quaternion _originalLocalRot;
        
        private Tween _posTween;
        private Tween _rotTween;
        private float _intensity = 1f;
        
        public CameraShaker(Camera camera)
        {
            _cameraTransform = camera.transform;
            _originalLocalPos = _cameraTransform.localPosition;
            _originalLocalRot = _cameraTransform.localRotation;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetIntensity(float intensity)
        {
            _intensity = intensity;
            if (_intensity == 0f)
                Stop();
        }

        public void DoShake(CameraShakePreset preset)
        {
            if (_intensity == 0f) return;
            Stop();

            _posTween = _cameraTransform.DOShakePosition(
                    preset.Duration,
                    preset.Strength * _intensity,
                    preset.Vibrato,
                    preset.Randomness,
                    snapping: false,
                    fadeOut: true)
                .SetTarget(_cameraTransform)
                .OnKill(ResetPosition)
                .OnComplete(ResetPosition);
            
            if (!preset.EnableRotationShake) return;

            _rotTween = _cameraTransform.DOShakeRotation(
                    preset.Duration,
                    preset.RotStrength * _intensity,
                    preset.RotVibrato,
                    preset.Randomness,
                    fadeOut: true)
                .SetTarget(_cameraTransform)
                .OnKill(ResetRotation)
                .OnComplete(ResetRotation);
        }

        public void DoShake(float duration, float strength, int vibrato = 10)
        {
            if (_intensity == 0f) return;
            Stop();

            _posTween = _cameraTransform.DOShakePosition(duration, strength * _intensity, vibrato)
                .SetTarget(_cameraTransform)
                .OnKill(ResetPosition)
                .OnComplete(ResetPosition);
        }
        
        private void Stop()
        {
            if (_posTween != null && _posTween.IsActive()) _posTween.Kill();
            if (_rotTween != null && _rotTween.IsActive()) _rotTween.Kill();

            ResetPosition();
            ResetRotation();
        }
        
        private void ResetPosition()
        {
            if (_cameraTransform != null)
                _cameraTransform.localPosition = _originalLocalPos;
        }

        private void ResetRotation()
        {
            if (_cameraTransform != null)
                _cameraTransform.localRotation = _originalLocalRot;
        }

        public void Dispose() => Stop();
    }
}
