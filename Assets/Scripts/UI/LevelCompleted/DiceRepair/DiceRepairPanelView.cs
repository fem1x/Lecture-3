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
        private Sequence _slotReveal;

        [Inject]
        public void Construct(
            RepairTokensManager repairTokensManager,
            DiceTableController diceTableController)
        {
            _repairTokensManager = repairTokensManager;
            _diceTableController = diceTableController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void OnDisable() => _slotReveal?.Kill();

        public void SetRepairMode(bool enable)
        {
            _slotReveal?.Kill();

            if (enable && !_isBound)
                BindDices();

            if (enable)
            {
                foreach (var view in _repairViews)
                    view.PrepareReveal();
            }

            var count = Mathf.Min(_repairViews.Count, _diceTableController.AllDices.Count);
            for (var i = 0; i < count; i++)
                _repairViews[i].SetRepairMode(enable);
        }

        internal async UniTask RevealSlotsAsync(float slideDuration, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            if (_repairViews.Count == 0)
                return;

            var delay = slideDuration * (_revealStartPercent / 100f);
            var sequence = DOTween.Sequence().SetUpdate(true);
            _slotReveal = sequence;
            for (var i = 0; i < _repairViews.Count; i++)
            {
                var start = delay + i * _revealStagger;
                _repairViews[i].InsertReveal(sequence, start, _revealDuration, _revealFadeEase, _revealScaleEase);
            }

            try
            {
                await sequence.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);
            }
            finally
            {
                if (_slotReveal == sequence)
                    _slotReveal = null;
            }
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
