using System;
using System.Threading;
using _Scripts.Configs;
using _Scripts.Dices;
using _Scripts.UI.Tooltips;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelRewardsView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private SlidingPanelView _slidingPanel;
        
        [Header("Texts")]
        [SerializeField] private TextRevealView _title;
        [SerializeField] private TMP_Text _totalTokensText;
        [SerializeField] private TMP_Text _levelTokensText;
        [SerializeField] private TMP_Text _throwsTokensText;
        [SerializeField] private TMP_Text _rerollsTokensText;

        [Header("Rewards Tooltip")]
        [SerializeField] private BasicTooltipTrigger _rewardsTooltip;
        [SerializeField] private TokenRewardConfig _tokenRewardConfig;

        [Header("Buttons")]
        [SerializeField] private Button _workshopButton;

        private Tween _titleEntrance;

        internal RectTransform WorkshopButtonRect => (RectTransform)_workshopButton.transform;

        public event Action OnWorkshopRequested;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _rewardsTooltip.TextFormatter = FormatRewardsTooltip;
            HideInstant();
        }

        private void OnDestroy() => _rewardsTooltip.TextFormatter = null;

        private string FormatRewardsTooltip(string text) => text
            .Replace("{throwsPerToken}", _tokenRewardConfig.ThrowsPerToken.ToString())
            .Replace("{rerollsPerToken}", _tokenRewardConfig.RerollsPerToken.ToString());

        private void OnEnable() => _workshopButton.onClick.AddListener(HandleWorkshopButtonClick);
        private void OnDisable()
        {
            _workshopButton.onClick.RemoveListener(HandleWorkshopButtonClick);
            StopTitleEntrance();
            _rewardsTooltip.HideImmediately();
        }

        private void HandleWorkshopButtonClick()
        {
            if (_workshopButton.IsInteractable())
                OnWorkshopRequested?.Invoke();
        }

        public void Setup(TokenRewardData data)
        {
            _totalTokensText.text = $"+ {data.TotalTokens}";
            _levelTokensText.text = data.LevelTokens.ToString();
            _throwsTokensText.text = data.ThrowsTokens.ToString();
            _rerollsTokensText.text = data.RerollsTokens.ToString();
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            StopTitleEntrance();
            _workshopButton.interactable = true;
            var titleEntrance = _titleEntrance = _title.Show();
            try
            {
                await UniTask.WhenAll(
                    _slidingPanel.ShowAsync(ct),
                    titleEntrance.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, ct));
            }
            finally
            {
                if (ReferenceEquals(_titleEntrance, titleEntrance))
                {
                    _titleEntrance = null;
                    _title.Restore();
                }
            }

            if (isActiveAndEnabled && _workshopButton.IsInteractable())
                _rewardsTooltip.ShowOnFirstVisit();
        }

        internal void BeginWorkshopTransition()
        {
            LockInteraction();
            StopTitleEntrance();
            _slidingPanel.StopSlide();
        }

        public void HideInstant()
        {
            LockInteraction();
            StopTitleEntrance();
            _slidingPanel.HideInstant();
        }

        private void StopTitleEntrance()
        {
            if (_titleEntrance == null) return;

            var entrance = _titleEntrance;
            _titleEntrance = null;
            _title.Restore();
            entrance.Kill();
        }

        private void LockInteraction()
        {
            _rewardsTooltip.HideImmediately();
            _workshopButton.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }
}
