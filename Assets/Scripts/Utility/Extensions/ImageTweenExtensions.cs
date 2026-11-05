using DG.Tweening;
using UnityEngine.UI;

namespace _Scripts.Utility.Extensions
{
    public static class ImageTweenExtensions
    {
        public static Tween DoFadeIn(this Image target, float toAlpha=1f, float time=0.5f, Ease ease = Ease.OutSine)
        {
            target.DOKill(true);
            
            target.SetVisible(false);
            return target.DOFade(toAlpha, time).SetEase(ease);
        }
        
        public static Tween DoFadeOut(this Image target, float toAlpha=0f, float time=0.5f, Ease ease = Ease.OutSine)
        {
            target.DOKill(true);
            
            return target.DOFade(toAlpha, time);
        }
    }
}