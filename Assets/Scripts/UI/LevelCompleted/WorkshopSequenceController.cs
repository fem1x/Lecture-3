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
        [SerializeField] private float _fastReturnThreshold = 0.25f;

        [Header("Upgrade Mode")]
        [SerializeField] private List<SlideStep> _upgradeShowSteps = new();
        [SerializeField] private List<SlideStep> _upgradeHideSteps = new();

        private float _mainHideStartTime = -1f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void HideMainInstant()
        {
            _mainHideStartTime = -1f;
            HideInstant(_mainShowSteps);
        }

        public void HideUpgradeInstant() => HideInstant(_upgradeShowSteps);

        public UniTask ShowMainAsync(CancellationToken ct)
        {
            var fastReturn = _mainHideStartTime >= 0f &&
                             Time.unscaledTime - _mainHideStartTime < _fastReturnThreshold;

            _mainHideStartTime = -1f;
            return RunStepsAsync(_mainShowSteps, show: true, ignoreDelays: fastReturn, ct);
        }

        public UniTask HideMainAsync(CancellationToken ct)
        {
            _mainHideStartTime = Time.unscaledTime;
            return RunStepsAsync(_mainHideSteps, show: false, ignoreDelays: false, ct);
        }

        public UniTask ShowUpgradeAsync(CancellationToken ct) => RunStepsAsync(_upgradeShowSteps, show: true, ignoreDelays: false, ct);

        public UniTask HideUpgradeAsync(CancellationToken ct) => RunStepsAsync(_upgradeHideSteps, show: false, ignoreDelays: false, ct);

        private void HideInstant(List<SlideStep> steps)
        {
            for (var i = 0; i < steps.Count; i++)
                steps[i].Slide.HideInstant();
        }

        private async UniTask RunStepsAsync(List<SlideStep> steps, bool show, bool ignoreDelays, CancellationToken ct)
        {
            var tasks = new List<UniTask>(steps.Count);

            for (var i = 0; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = steps[i];
                if (!ignoreDelays && step.DelayBeforeStart > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(step.DelayBeforeStart), ignoreTimeScale: true, cancellationToken: ct);

                ct.ThrowIfCancellationRequested();
                tasks.Add(show ? step.Slide.ShowAsync() : step.Slide.HideAsync());
            }

            await UniTask.WhenAll(tasks);
        }
    }
}