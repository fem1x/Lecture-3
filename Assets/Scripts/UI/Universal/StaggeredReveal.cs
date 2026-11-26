using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    internal sealed class StaggeredReveal
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly (CanvasGroup Group, Vector3 ShownScale)[] _items;
        private readonly bool _blockInteraction;
        private Sequence _sequence;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public StaggeredReveal(IReadOnlyList<MonoBehaviour> views, bool blockInteraction = true)
        {
            _blockInteraction = blockInteraction;
            _items = new (CanvasGroup, Vector3)[views.Count];
            for (var i = 0; i < views.Count; i++)
            {
                var view = views[i];
                _items[i] = (view.gameObject.AddComponent<CanvasGroup>(), view.transform.localScale);
            }
        }

        public void Cancel() => _sequence?.Kill();

        public void Prepare()
        {
            Cancel();

            foreach (var (group, shownScale) in _items)
            {
                group.alpha = 0f;
                group.blocksRaycasts = !_blockInteraction;
                group.interactable = !_blockInteraction;
                group.transform.localScale = shownScale * 0.85f;
            }
        }

        public async UniTask PlayAsync(
            float delay, float duration, float stagger, Ease fadeEase, Ease scaleEase, CancellationToken ct,
            Action<Sequence, int, float> insertChildReveal = null)
        {
            ct.ThrowIfCancellationRequested();
            if (_items.Length == 0)
                return;

            var sequence = DOTween.Sequence().SetUpdate(true);
            _sequence = sequence;
            InsertInto(sequence, delay, duration, stagger, fadeEase, scaleEase, insertChildReveal);

            try
            {
                await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);
            }
            finally
            {
                if (_sequence == sequence)
                    _sequence = null;
            }
        }

        public void InsertInto(
            Sequence sequence, float delay, float duration, float stagger, Ease fadeEase, Ease scaleEase,
            Action<Sequence, int, float> insertChildReveal = null)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                var (group, shownScale) = _items[i];
                var start = delay + i * stagger;
                sequence.Insert(start, group.DOFade(1f, duration).SetEase(fadeEase));
                sequence.Insert(start, group.transform.DOScale(shownScale, duration).SetEase(scaleEase));

                if (_blockInteraction)
                {
                    sequence.InsertCallback(start + duration, () =>
                    {
                        group.blocksRaycasts = true;
                        group.interactable = true;
                    });
                }

                insertChildReveal?.Invoke(sequence, i, start);
            }
        }
    }
}
