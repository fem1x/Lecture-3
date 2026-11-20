using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.UI.Workshop
{
    [Serializable]
    public struct SlideStep
    {
        [field: SerializeField] public SlidingPanelView Slide { get; private set; }
        [field: SerializeField] public float DelayBeforeStart { get; private set; }
    }

    public class WorkshopSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Main Mode")]
        [SerializeField] private List<SlideStep> _mainShowSteps = new();
        [SerializeField] private List<SlideStep> _mainHideSteps = new();

        [Header("Upgrade Mode")]
        [SerializeField] private List<SlideStep> _upgradeShowSteps = new();
        [SerializeField] private List<SlideStep> _upgradeHideSteps = new();
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void HideMainInstant() => HideInstant(_mainShowSteps);

        public void HideUpgradeInstant() => HideInstant(_upgradeShowSteps);

        public UniTask ShowMainAsync(CancellationToken ct) => RunStepsAsync(_mainShowSteps, show: true, ct);

        public UniTask HideMainAsync(CancellationToken ct) => RunStepsAsync(_mainHideSteps, show: false, ct);

        public UniTask ShowUpgradeAsync(CancellationToken ct) => RunStepsAsync(_upgradeShowSteps, show: true, ct);

        public UniTask HideUpgradeAsync(CancellationToken ct) => RunStepsAsync(_upgradeHideSteps, show: false, ct);

        private void HideInstant(List<SlideStep> steps)
        {
            for (var i = 0; i < steps.Count; i++)
                steps[i].Slide.HideInstant();
        }

        private async UniTask RunStepsAsync(List<SlideStep> steps, bool show, CancellationToken ct)
        {
            var tasks = new List<UniTask>(steps.Count);

            for (var i = 0; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = steps[i];
                if (step.DelayBeforeStart > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(step.DelayBeforeStart), ignoreTimeScale: true, cancellationToken: ct);

                ct.ThrowIfCancellationRequested();
                tasks.Add(show ? step.Slide.ShowAsync() : step.Slide.HideAsync());
            }

            await UniTask.WhenAll(tasks);
        }
    }
}