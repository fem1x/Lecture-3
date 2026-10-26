using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility.Extensions
{
    public static class TransformTweenExtensions
    {
        public static Tween DoScalePop(this Transform target, float scaleMult = 1.3f, float inTime = 0.07f, float outTime = 0.25f)
        {
            target.DOKill(true);
            var startingScale = target.localScale;
            
            return DOTween.Sequence()
                .Append(target.DOScale(startingScale * scaleMult, inTime).SetEase(Ease.OutQuad))
                .Append(target.DOScale(startingScale, outTime).SetEase(Ease.InQuad));
        }
    }
}