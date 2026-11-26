using System.Collections.Generic;
using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceRepairPanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<DiceRepairView> _repairViews = new();

        [Header("Slot Reveal Animation")]
        [SerializeField, Range(0f, 100f)]
        [Tooltip("Start time as a percentage of the repair panel slide duration. 0 starts with the slide, 100 starts when it ends.")]
        private float _revealStartPercent = 65f;
        [SerializeField, Min(0.01f), Tooltip("Reveal duration for each slot in seconds.")]
        private float _revealDuration = 0.26f;
        [SerializeField, Min(0f), Tooltip("Seconds between the start of each slot's reveal.")]
        private float _revealStagger = 0.065f;
        [SerializeField] private Ease _revealFadeEase = Ease.OutQuad;
        [SerializeField] private Ease _revealScaleEase = Ease.OutCubic;

        private RepairTokensManager _repairTokensManager;
        private DiceTableController _diceTableController;
        private bool _isBound;
        private StaggeredReveal _slotReveal;

        [Inject]
        public void Construct(
            RepairTokensManager repairTokensManager,
            DiceTableController diceTableController)
        {
            _repairTokensManager = repairTokensManager;
            _diceTableController = diceTableController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => _slotReveal = new StaggeredReveal(_repairViews);

        private void OnDisable() => _slotReveal.Cancel();

        public void SetRepairMode(bool enable)
        {
            if (enable && !_isBound)
                BindDices();

            if (enable)
                _slotReveal.Prepare();
            else
                _slotReveal.Cancel();

            var count = Mathf.Min(_repairViews.Count, _diceTableController.AllDices.Count);
            for (var i = 0; i < count; i++)
                _repairViews[i].SetRepairMode(enable);
        }

        internal UniTask RevealSlotsAsync(float slideDuration, CancellationToken ct)
        {
            var delay = slideDuration * (_revealStartPercent / 100f);
            return _slotReveal.PlayAsync(delay, _revealDuration, _revealStagger, _revealFadeEase, _revealScaleEase, ct);
        }

        private void BindDices()
        {
            var dices = _diceTableController.AllDices;
            var count = Mathf.Min(_repairViews.Count, dices.Count);

            for (var i = 0; i < count; i++)
                _repairViews[i].Bind(dices[i].Data);

            _isBound = true;
        }
    }
}
