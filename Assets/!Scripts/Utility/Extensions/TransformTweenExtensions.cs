using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility.Extensions
{
    public static class TransformTweenExtensions
    {
        public static Tween DoScalePop(this Transform target, float scaleMult = 1.3f, float inTime = 0.07f, float outTime = 0.25f)
        {
            target.DOKill(true);
            var targetScale = target.localScale;
            
            return DOTween.Sequence()
                .Append(target.DOScale(targetScale * scaleMult, inTime).SetEase(Ease.OutQuad))
                .Append(target.DOScale(targetScale, outTime).SetEase(Ease.InQuad))
                .SetLink(target.gameObject);
        }

        public static Tween DoScaleIn(this Transform target, float time = 0.5f, Ease ease = Ease.OutQuart)
        {
            target.DOKill(true);
            var targetScale = target.localScale;
            
            target.localScale =  Vector3.zero;
            return target.DOScale(targetScale, time).SetEase(ease).SetLink(target.gameObject);
        }
    }
}