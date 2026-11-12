using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Utility.Extensions
{
    public static class TransformTweenExtensions
    {
        private class ScaleCache
        {
            public readonly Vector3 InitialScale;
            public ScaleCache(Vector3 scale) => InitialScale = scale;
        }
        
        private static readonly ConditionalWeakTable<Transform, ScaleCache> _initialScales = new();

        public static Tween DoScalePop(
            this Transform target, 
            float scaleMult = 1.3f, 
            float inTime = 0.07f, 
            float outTime = 0.5f)
        {
            var baseScale = _initialScales.GetValue(target, t => new ScaleCache(t.localScale)).InitialScale;

            target.DOKill();
            target.localScale = baseScale;

            return DOTween.Sequence()
                .Append(target.DOScale(baseScale * scaleMult, inTime).SetEase(Ease.OutQuad))
                .Append(target.DOScale(baseScale, outTime).SetEase(Ease.InQuad))
                .SetLink(target.gameObject);
        }

        public static Tween DoScaleIn(this Transform target, float time = 0.5f, Ease ease = Ease.OutQuart)
        {
            target.DOKill(true);
            var targetScale = target.localScale;
            
            target.localScale =  Vector3.zero;
            return target.DOScale(targetScale, time).SetEase(ease).SetLink(target.gameObject);
        }
        
        public static Tween DoScaleOut(this Transform target, float time = 0.5f, Ease ease = Ease.OutQuart)
        {
            target.DOKill(true);
            
            return target.DOScale(Vector3.zero, time).SetEase(ease).SetLink(target.gameObject);
        }
        
        public static Tween DoFlipX(this Transform target, Action onHalfway, float time = 0.4f, Ease easeIn = Ease.InQuad, Ease easeOut = Ease.OutBack)
        {
            var halfTime = time * 0.5f;
            
            return DOTween.Sequence()
                .Append(target.DOScaleX(0f, halfTime).SetEase(easeIn))
                .AppendCallback(() => 
                {
                    if (target != null) onHalfway?.Invoke();
                })
                .Append(target.DOScaleX(1f, halfTime).SetEase(easeOut))
                .SetTarget(target)
                .SetLink(target.gameObject);
        }
        
        #region Folding
        public static Tween DoUnfoldX(this Transform target, float targetScaleX = 1f, float time = 0.4f, Ease ease = Ease.OutBack)
        {
            target.DOKill(true);
            var currentScale = target.localScale;

            target.localScale = new(0f, currentScale.y, currentScale.z);
            return target.DOScaleX(targetScaleX, time).SetEase(ease).SetLink(target.gameObject);
        }

        public static Tween DoFoldX(this Transform target, float time = 0.3f, Ease ease = Ease.InQuart)
        {
            target.DOKill(true);

            return target.DOScaleX(0f, time).SetEase(ease).SetLink(target.gameObject);
        }
        #endregion
    }
}