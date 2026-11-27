using System;
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
        [SerializeField] private TextRevealView _allRepairedText;

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
        private Tween _allRepairedTween;
        private bool _repairModeEnabled;
        private bool _messageReady;
        private bool _messageShown;

        [Inject]
        public void Construct(
            RepairTokensManager repairTokensManager,
            DiceTableController diceTableController)
        {
            _repairTokensManager = repairTokensManager;
            _diceTableController = diceTableController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _slotReveal = new StaggeredReveal(_repairViews);
            _allRepairedText.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _slotReveal.Cancel();
            HideAllRepaired();
        }

        private void OnDestroy()
        {
            if (!_isBound) return;
            foreach (var dice in _diceTableController.AllDices)
                dice.Data.OnDataChanged -= RefreshAllRepaired;
        }

        public void SetRepairMode(bool enable)
        {
            _repairModeEnabled = enable;
            _messageReady = false;

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

        internal async UniTask RevealSlotsAsync(float slideDuration, CancellationToken ct)
        {
            var delay = slideDuration * (_revealStartPercent / 100f);
            var slots = _slotReveal.PlayAsync(delay, _revealDuration, _revealStagger, _revealFadeEase, _revealScaleEase, ct);
            if (delay > 0f)
                await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true, cancellationToken: ct);
            ct.ThrowIfCancellationRequested();
            _messageReady = true;
            RefreshAllRepaired();
            await slots;
        }

        private void BindDices()
        {
            var dices = _diceTableController.AllDices;
            var count = Mathf.Min(_repairViews.Count, dices.Count);

            for (var i = 0; i < count; i++)
                _repairViews[i].Bind(dices[i].Data);

            foreach (var dice in dices)
                dice.Data.OnDataChanged += RefreshAllRepaired;

            _isBound = true;
        }

        private void RefreshAllRepaired()
        {
            if (!_repairModeEnabled || !_messageReady) return;

            var dices = _diceTableController.AllDices;
            var allRepaired = dices.Count > 0 && dices.TrueForAll(dice => !dice.Data.NeedsRepair);
            if (!allRepaired)
            {
                HideAllRepaired();
                return;
            }

            if (_messageShown) return;
            _messageShown = true;
            _allRepairedText.gameObject.SetActive(true);
            var textTransform = _allRepairedText.transform;
            textTransform.localScale = Vector3.one * 0.88f;
            _allRepairedTween = DOTween.Sequence().SetUpdate(true)
                .Join(_allRepairedText.Show())
                .Join(textTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
        }

        private void HideAllRepaired()
        {
            _allRepairedTween?.Kill();
            _allRepairedTween = null;
            _allRepairedText.gameObject.SetActive(false);
            _allRepairedText.transform.localScale = Vector3.one;
            _messageShown = false;
        }
    }
}
