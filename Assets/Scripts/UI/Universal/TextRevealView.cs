using DG.Tweening;
using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TextRevealView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _text;
        [SerializeField, Min(0f), Tooltip("Total time in seconds for all characters to appear and settle.")]
        private float _showDuration = 1f;
        [SerializeField, Tooltip("Controls the pace of the whole reveal from the first character to the last.")]
        private Ease _showEase = Ease.OutQuart;
        [SerializeField, Min(0f), Tooltip("Total time in seconds to erase the text from end to start.")]
        private float _hideDuration = 0.2f;
        [SerializeField, Tooltip("Controls how the erasing speeds up or slows down over the hide duration.")]
        private Ease _hideEase = Ease.InCubic;

        [Header("Character Entrance")]
        [SerializeField, Range(0.05f, 1f), Tooltip("Share of the reveal timeline used by each character. Higher values make their animations overlap more. At 1 they start together.")]
        private float _characterDurationFraction = 0.2f;
        [SerializeField, Min(0f), Tooltip("How far below its final position each character starts, measured in its own height. For example, 0.35 means 35% of its height.")]
        private float _riseInGlyphHeights = 0.35f;
        [SerializeField, Range(0.1f, 1f), Tooltip("Starting size relative to the final size. For example, 0.25 means 25% of full size.")]
        private float _startScale = 0.25f;
        [SerializeField, Tooltip("Controls the rise and scale of each individual character. OutBack briefly overshoots before settling.")]
        private Ease _characterEase = Ease.OutBack;

        public float ShowDuration => _showDuration;
        public float HideDuration => _hideDuration;

        private int _originalVisibleCharacters;
        private bool _isRevealing;
        private float _revealProgress;
        private TMP_MeshInfo[] _originalMesh;
        private bool _isInitialized;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => EnsureInitialized();

        private void EnsureInitialized()
        {
            if (_isInitialized) return;
            _originalVisibleCharacters = _text.maxVisibleCharacters;
            _isInitialized = true;
        }

        private void OnDisable() => Restore();

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public Tween Show(float? duration = null)
        {
            Restore();
            _isRevealing = true;
            _revealProgress = 0f;
            _text.OnPreRenderText += PrepareRevealMesh;
            _text.ForceMeshUpdate();

            return DOVirtual.Float(0f, 1f, duration ?? _showDuration, UpdateReveal)
                .SetEase(_showEase).SetUpdate(true).SetLink(gameObject)
                .OnComplete(Restore);
        }

        public Tween Hide(float? duration = null)
        {
            if (_isRevealing)
                Restore();

            _text.ForceMeshUpdate();
            var count = Mathf.Min(_text.textInfo.characterCount, _originalVisibleCharacters);
            _text.maxVisibleCharacters = Mathf.Min(_text.maxVisibleCharacters, count);
            return Animate(0, count, duration ?? _hideDuration, _hideEase);
        }

        public void Restore()
        {
            EnsureInitialized();
            var wasRevealing = _isRevealing;
            if (wasRevealing)
            {
                _isRevealing = false;
                _text.OnPreRenderText -= PrepareRevealMesh;
                _originalMesh = null;
            }

            _text.maxVisibleCharacters = _originalVisibleCharacters;
            if (wasRevealing && _text.isActiveAndEnabled)
                _text.ForceMeshUpdate();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void PrepareRevealMesh(TMP_TextInfo info)
        {
            _originalMesh = info.CopyMeshInfoVertexData();
            ApplyReveal(info);
        }

        private void UpdateReveal(float progress)
        {
            if (!_isRevealing) return;

            _revealProgress = Mathf.Clamp01(progress);
            if (_originalMesh == null) return;

            ApplyReveal(_text.textInfo);
            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
        }

        private void ApplyReveal(TMP_TextInfo info)
        {
            var count = Mathf.Min(info.characterCount, _originalVisibleCharacters);
            var duration = count > 1 ? _characterDurationFraction : 1f;
            var stagger = count > 1 ? (1f - duration) / (count - 1) : 0f;

            for (var i = 0; i < count; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible) continue;

                var progress = Mathf.Clamp01((_revealProgress - i * stagger) / duration);
                var motion = DOVirtual.EasedValue(0f, 1f, progress, _characterEase);
                var opacity = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress * 2f));
                var scale = Mathf.LerpUnclamped(_startScale, 1f, motion);

                var material = character.materialReferenceIndex;
                var first = character.vertexIndex;
                var original = _originalMesh[material];
                var mesh = info.meshInfo[material];
                var bottom = original.vertices[first];
                var top = original.vertices[first + 2];
                var pivot = (bottom + top) * 0.5f;
                var offset = Vector3.down * ((top.y - bottom.y) * _riseInGlyphHeights * (1f - motion));

                for (var vertex = first; vertex < first + 4; vertex++)
                {
                    mesh.vertices[vertex] = pivot + (original.vertices[vertex] - pivot) * scale + offset;
                    var color = original.colors32[vertex];
                    color.a = (byte)Mathf.RoundToInt(color.a * opacity);
                    mesh.colors32[vertex] = color;
                }
            }
        }

        private Tween Animate(int target, int count, float duration, Ease ease)
        {
            return DOTween.To(() => _text.maxVisibleCharacters,
                    value => _text.maxVisibleCharacters = Mathf.Clamp(value, 0, count), target, duration)
                .SetEase(ease).SetUpdate(true).SetLink(gameObject);
        }
    }
}
