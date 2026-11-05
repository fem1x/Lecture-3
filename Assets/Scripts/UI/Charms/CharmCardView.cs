using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Charms
{
    public class CharmCardView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Components")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _backdropButton;

        private static readonly int ShowTrigger = Animator.StringToHash("Open");
        private static readonly int HideTrigger = Animator.StringToHash("Close");
        private static readonly int HiddenStateHash = Animator.StringToHash("Hidden");

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
        }

        private void OnEnable() => _backdropButton.onClick.AddListener(HandleCloseClicked);

        private void OnDisable() => _backdropButton.onClick.RemoveListener(HandleCloseClicked);

        public async UniTask ShowAsync(CharmConfigBase config, CancellationToken ct)
        {
            _nameText.text = config.Name.GetLocalizedString();
            _descriptionText.text = config.GetDescription();
            _iconImage.sprite = config.Icon;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            _animator.ResetTrigger(HideTrigger);
            _animator.SetTrigger(ShowTrigger);

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        public async UniTask HideAsync(CancellationToken ct)
        {
            _canvasGroup.interactable = false;

            _animator.ResetTrigger(ShowTrigger);
            _animator.SetTrigger(HideTrigger);

            await UniTask.WaitUntil(() =>
            {
                var stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.shortNameHash == HiddenStateHash;
            }, cancellationToken: ct);

            _canvasGroup.blocksRaycasts = false;
        }

        private void HandleCloseClicked() => HideAsync(destroyCancellationToken).Forget();
    }
}