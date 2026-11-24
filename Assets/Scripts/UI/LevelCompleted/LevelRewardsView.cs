using System;
using System.Threading;
using _Scripts.Dices;
using Cysharp.Threading.Tasks;
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
        [SerializeField] private TMP_Text _totalTokensText;
        [SerializeField] private TMP_Text _levelTokensText;
        [SerializeField] private TMP_Text _throwsTokensText;
        [SerializeField] private TMP_Text _rerollsTokensText;

        [Header("Buttons")]
        [SerializeField] private Button _workshopButton;

        internal RectTransform WorkshopButtonRect => (RectTransform)_workshopButton.transform;

        public event Action OnWorkshopRequested;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => HideInstant();

        private void OnEnable() => _workshopButton.onClick.AddListener(HandleWorkshopButtonClick);
        private void OnDisable() => _workshopButton.onClick.RemoveListener(HandleWorkshopButtonClick);

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

        public UniTask ShowAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _workshopButton.interactable = true;
            return _slidingPanel.ShowAsync(ct);
        }

        internal void BeginWorkshopTransition()
        {
            LockInteraction();
            _slidingPanel.StopSlide();
        }

        public void HideInstant()
        {
            LockInteraction();
            _slidingPanel.HideInstant();
        }

        private void LockInteraction()
        {
            _workshopButton.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }
    }
}