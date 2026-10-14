using System;
using System.Threading;
using _Scripts.Dices;
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
        [Header("Texts")]
        [SerializeField] private TMP_Text _totalTokensText;
        [SerializeField] private TMP_Text _levelTokensText;
        [SerializeField] private TMP_Text _throwsTokensText;
        [SerializeField] private TMP_Text _rerollsTokensText;

        [Header("Buttons")]
        [SerializeField] private Button _workshopButton;

        [Header("Fade Settings")]
        [SerializeField] private float _fadeDuration = 0.25f;

        private CanvasGroup _canvasGroup;

        public event Action OnWorkshopRequested;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            HideInstant();
        }

        private void OnEnable() => _workshopButton.onClick.AddListener(HandleWorkshopButtonClick);
        private void OnDisable() => _workshopButton.onClick.RemoveListener(HandleWorkshopButtonClick);

        private void HandleWorkshopButtonClick() => OnWorkshopRequested?.Invoke();

        public void Setup(TokenRewardData data)
        {
            _totalTokensText.text = $"+ {data.TotalTokens}";
            _levelTokensText.text = data.LevelTokens.ToString();
            _throwsTokensText.text = data.ThrowsTokens.ToString();
            _rerollsTokensText.text = data.RerollsTokens.ToString();
        }

        public async UniTask ShowAsync(CancellationToken ct)
        {
            _canvasGroup.DOKill();
            SetInteractable(true);
            await _canvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true).WithCancellation(ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            _canvasGroup.DOKill();
            SetInteractable(false);
            await _canvasGroup.DOFade(0f, _fadeDuration).SetUpdate(true).WithCancellation(ct);
        }

        public void HideInstant()
        {
            SetInteractable(false);
            _canvasGroup.alpha = 0f;
        }

        private void SetInteractable(bool active)
        {
            _canvasGroup.blocksRaycasts = active;
            _canvasGroup.interactable = active;
        }
    }
}