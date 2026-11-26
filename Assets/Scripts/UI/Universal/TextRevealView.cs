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
        [SerializeField, Min(0f)] private float _showDuration = 0.4f;
        [SerializeField] private Ease _showEase = Ease.Linear;
        [SerializeField, Min(0f)] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _hideEase = Ease.Linear;

        private int _originalVisibleCharacters;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _originalVisibleCharacters = _text.maxVisibleCharacters;
        }

        public Tween Show()
        {
            _text.ForceMeshUpdate();
            var count = Mathf.Min(_text.textInfo.characterCount, _originalVisibleCharacters);
            _text.maxVisibleCharacters = 0;
            return Animate(count, count, _showDuration, _showEase)
                .OnComplete(Restore);
        }

        public Tween Hide()
        {
            _text.ForceMeshUpdate();
            var count = Mathf.Min(_text.textInfo.characterCount, _originalVisibleCharacters);
            _text.maxVisibleCharacters = Mathf.Min(_text.maxVisibleCharacters, count);
            return Animate(0, count, _hideDuration, _hideEase);
        }

        public void Restore() => _text.maxVisibleCharacters = _originalVisibleCharacters;

        private Tween Animate(int target, int count, float duration, Ease ease)
        {
            return DOTween.To(() => _text.maxVisibleCharacters,
                    value => _text.maxVisibleCharacters = Mathf.Clamp(value, 0, count), target, duration)
                .SetEase(ease).SetUpdate(true);
        }
    }
}
