using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class FadeOutPanelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Ease _fadeEase = Ease.InOutCubic;

        private void Awake() => _canvasGroup.alpha = 1f;
        private void Start() => DoFadeTween();
        private void DoFadeTween()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.DOFade(0f, _fadeDuration).SetEase(_fadeEase);
        }
    }
}