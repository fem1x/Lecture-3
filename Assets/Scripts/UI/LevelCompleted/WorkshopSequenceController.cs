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

        internal SlidingPanelView FirstMainSlide => _mainShowSteps[0].Slide;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void HideMainInstant()
        {
            _mainHideStartTime = -1f;
            HideInstant(_mainShowSteps);
        }

        public void HideUpgradeInstant() => HideInstant(_upgradeShowSteps);

        public UniTask ShowMainAsync(DiceRepairPanelView repairPanel, CancellationToken ct)
        {
            var fastReturn = _mainHideStartTime >= 0f &&
                             Time.unscaledTime - _mainHideStartTime < _fastReturnThreshold;

            _mainHideStartTime = -1f;
            return RunStepsAsync(_mainShowSteps, show: true, ignoreDelays: fastReturn, ct,
                repairPanel.transform, repairPanel.RevealSlotsAsync);
        }

        public UniTask HideMainAsync(CancellationToken ct)
        {
            _mainHideStartTime = Time.unscaledTime;
            return RunStepsAsync(_mainHideSteps, show: false, ignoreDelays: false, ct);
        }

        public UniTask ShowUpgradeAsync(UpgradePanelView upgradePanel, CancellationToken ct) =>
            RunStepsAsync(_upgradeShowSteps, show: true, ignoreDelays: false, ct,
                upgradePanel.transform, upgradePanel.RevealSectionsAsync);

        public UniTask HideUpgradeAsync(CancellationToken ct) => RunStepsAsync(_upgradeHideSteps, show: false, ignoreDelays: false, ct);

        private void HideInstant(List<SlideStep> steps)
        {
            for (var i = 0; i < steps.Count; i++)
                steps[i].Slide.HideInstant();
        }

        private async UniTask RunStepsAsync(
            List<SlideStep> steps,
            bool show,
            bool ignoreDelays,
            CancellationToken ct,
            Transform revealTarget = null,
            Func<float, CancellationToken, UniTask> reveal = null)
        {
            var tasks = new List<UniTask>(steps.Count);

            for (var i = 0; i < steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var step = steps[i];
                if (!ignoreDelays && step.DelayBeforeStart > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(step.DelayBeforeStart), ignoreTimeScale: true, cancellationToken: ct);

                ct.ThrowIfCancellationRequested();
                tasks.Add(show ? step.Slide.ShowAsync(ct) : step.Slide.HideAsync(ct));
                if (reveal != null && step.Slide.Content == revealTarget)
                    tasks.Add(reveal(step.Slide.ShowDuration, ct));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
