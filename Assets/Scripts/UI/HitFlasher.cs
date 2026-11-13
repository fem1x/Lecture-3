using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility
{
    public class HitFlasher : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Renderer _renderer;
        [SerializeField] private string _colorProperty = "_EmissionColor";

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _propBlock;
        private Tween _flashTween;
        private int _propertyId;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _propBlock = new();
            _propertyId = string.IsNullOrEmpty(_colorProperty)
                ? EmissionColorId
                : Shader.PropertyToID(_colorProperty);
        }

        private void OnDisable() => StopImmediate();

        private void OnDestroy() => _flashTween?.Kill();

        public void Flash(Color flashColor, float time = 0.16f)
        {
            _flashTween?.Kill();

            var currentColor = flashColor;
            ApplyColor(currentColor);

            _flashTween = DOTween.To(
                    () => currentColor,
                    color =>
                    {
                        currentColor = color;
                        ApplyColor(currentColor);
                    },
                    Color.black,
                    time)
                .SetEase(Ease.OutQuad);
        }

        public void StopImmediate()
        {
            _flashTween?.Kill();
            if (_propBlock == null) return;

            ApplyColor(Color.black);
        }

        private void ApplyColor(Color color)
        {
            _renderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_propertyId, color);
            _renderer.SetPropertyBlock(_propBlock);
        }
    }
}