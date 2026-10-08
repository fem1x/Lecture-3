using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Charms
{
    public class CharmView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;

        public void Init(Sprite icon) => _iconImage.sprite = icon;

        public void PlayTriggerAnimation(float scaleMult = 1.35f, float duration = 0.2f)
        {
            transform.DOKill(true);
            var initialScale = Vector3.one;

            DOTween.Sequence()
                .Append(transform.DOScale(initialScale * scaleMult, duration * 0.35f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(initialScale, duration * 0.65f).SetEase(Ease.InQuad));
        }
    }
}