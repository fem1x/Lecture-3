using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectMask2D))]
    public sealed class PanelRevealView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField, Tooltip("The bottom edge of this header is where the reveal starts.")]
        private RectTransform _header;

        [Header("Reveal Animation")]
        [SerializeField, Min(0f), Tooltip("Seconds to hold at the header before opening.")]
        private float _showDelay = 0.05f;
        [SerializeField, Min(0f), Tooltip("Seconds to expand the clip from the header to the bottom of the panel.")]
        private float _showDuration = 0.75f;
        [SerializeField] private Ease _showEase = Ease.OutQuint;

        private RectMask2D _mask;
        private float _progress = 1f;

        private RectMask2D Mask => _mask != null ? _mask : _mask = GetComponent<RectMask2D>();
        internal bool IsRevealed => !Mask.enabled;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public Tween Show()
        {
            SetProgress(_progress);

            return DOVirtual.Float(_progress, 1f, _showDuration * (1f - _progress), SetProgress)
                .SetDelay(_progress == 0f ? _showDelay : 0f)
                .SetEase(_showEase)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(ShowInstant);
        }

        public void ShowInstant()
        {
            _progress = 1f;
            Mask.enabled = false;
        }

        public void HideInstant() => SetProgress(0f);

        private void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);
            var mask = Mask;
            var rect = mask.rectTransform;
            var headerBottom = _header.TransformPoint(0f, _header.rect.yMin, 0f);
            var hiddenHeight = rect.InverseTransformPoint(headerBottom).y - rect.rect.yMin;
            var hiddenFraction = Mathf.InverseLerp(0f, rect.rect.height, hiddenHeight);

            // RectMask2D applies its visual padding in canvas space.
            var padding = mask.padding;
            padding.y = mask.canvasRect.height * hiddenFraction * (1f - _progress);
            mask.padding = padding;
            mask.enabled = true;
        }
    }
}
