using System.Collections.Generic;
using System.Threading;
using _Scripts.Dices;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _Scripts.UI.Workshop
{
    public class UpgradePanelView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private List<UpgradeSectionView> _sections = new();

        [Header("Section Reveal Animation")]
        [SerializeField, Range(0f, 100f)]
        [Tooltip("Start time as a percentage of the upgrade panel slide duration. 0 starts with the slide, 100 starts when it ends.")]
        private float _revealStartPercent = 65f;
        [SerializeField] private StaggeredRevealView _sectionReveal;

        [Header("Face Reveal Animation")]
        [SerializeField, Min(0f), Tooltip("Seconds after a section starts revealing before its first face starts.")]
        private float _faceRevealDelay = 0.05f;
        [SerializeField, Min(0.01f), Tooltip("Reveal duration for each face button in seconds.")]
        private float _faceRevealDuration = 0.2f;
        [SerializeField, Min(0f), Tooltip("Seconds between the start of each face button's reveal.")]
        private float _faceRevealStagger = 0.05f;
        [SerializeField] private Ease _faceRevealFadeEase = Ease.OutQuad;
        [SerializeField] private Ease _faceRevealScaleEase = Ease.OutCubic;

        private DiceTableController _diceTableController;

        [Inject]
        public void Construct(DiceTableController diceTableController) 
            => _diceTableController = diceTableController;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void OnDisable() => _sectionReveal.Restore();

        public void BindSections()
        {
            var dices = _diceTableController.AllDices;
            var count = Mathf.Min(_sections.Count, dices.Count);

            for (var i = 0; i < count; i++)
                _sections[i].Bind(dices[i].Data);
        }

        internal async UniTask RevealSectionsAsync(float slideDuration, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _sectionReveal.Prepare();
            foreach (var section in _sections)
                section.FaceReveal.Prepare();

            var delay = slideDuration * (_revealStartPercent / 100f);
            await _sectionReveal.Show(delay, insertChildReveal: InsertFaceReveal)
                .ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct);
        }

        private void InsertFaceReveal(Sequence sequence, int sectionIndex, float sectionStart)
        {
            _sections[sectionIndex].FaceReveal.InsertInto(
                sequence, sectionStart + _faceRevealDelay, _faceRevealDuration, _faceRevealStagger,
                _faceRevealFadeEase, _faceRevealScaleEase);
        }
    }
}
