using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class TooltipViewBase : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
        [field: SerializeField] public float FadeDuration { get; private set; } = 0.12f;

        private Tween _fadeTween;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        protected virtual void Awake()
        {
            CanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDisable() => KillTween();

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public virtual void Show(RectTransform target)
        {
            KillTween();
            transform.position = target.position;
            gameObject.SetActive(true);

            _fadeTween = CanvasGroup.DOFade(1f, FadeDuration).SetUpdate(true);
        }

        public virtual void Hide()
        {
            KillTween();
            _fadeTween = CanvasGroup.DOFade(0f, FadeDuration)
                .SetUpdate(true)
                .OnComplete(() => gameObject.SetActive(false));
        }

        public void HideImmediate()
        {
            KillTween();
            CanvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void KillTween()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
        }
    }
}