using System;
using _Scripts.Dices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LevelRewardsView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Texts")]
        [SerializeField] private TMP_Text _totalTokensText;
        [SerializeField] private TMP_Text _levelTokensText;
        [SerializeField] private TMP_Text _throwsTokensText;
        [SerializeField] private TMP_Text _rerollsTokensText;
        
        [Header("Buttons")]
        [SerializeField] private Button _workshopButton;

        public event Action OnWorkshopRequested;
        
        private CanvasGroup _canvasGroup;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _canvasGroup = GetComponent<CanvasGroup>();

        private void OnEnable() => _workshopButton.onClick.AddListener(HandleWorkshopButtonClick);
        private void OnDisable() => _workshopButton.onClick.RemoveListener(HandleWorkshopButtonClick);

        public void Setup(TokenRewardData data)
        {
            _totalTokensText.text = $"+ {data.TotalTokens}";
            _levelTokensText.text = data.LevelTokens.ToString();
            _throwsTokensText.text = data.ThrowsTokens.ToString();
            _rerollsTokensText.text = data.RerollsTokens.ToString();
        }

        public void SetVisible(bool isVisible)
        {
            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.blocksRaycasts = isVisible;
            _canvasGroup.interactable = isVisible;
        }

        private void HandleWorkshopButtonClick() => OnWorkshopRequested?.Invoke();
    }
}