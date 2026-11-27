using _Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class ButtonSfxView : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;

        [Header("SFX Names")]
        [SerializeField] private string _hoverSfx = "ButtonHover";
        [SerializeField] private string _clickSfx = "ButtonClick";
        [SerializeField] private string _disabledClickSfx = "ButtonDisabledClick";

        private ISfxPlayer _sfxPlayer;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Initialize(Button button, ISfxPlayer sfxPlayer)
        {
            _button = button;
            _sfxPlayer = sfxPlayer;
        }

        private void Reset() => _button = GetComponent<Button>();

        private void OnEnable() => _button.onClick.AddListener(HandleClick);

        private void OnDisable() => _button.onClick.RemoveListener(HandleClick);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;
            _sfxPlayer.PlayUI(_hoverSfx, (RectTransform)_button.transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button.interactable) return;
            _sfxPlayer.PlayUI(_disabledClickSfx, (RectTransform)_button.transform);
        }

        private void HandleClick() => _sfxPlayer.PlayUI(_clickSfx, (RectTransform)_button.transform);
    }
}