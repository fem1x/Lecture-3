using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI.Charms
{
    public class CharmEquippedView : MonoBehaviour
    {
        [field: SerializeField] public CharmView CharmView { get; private set; }
        
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