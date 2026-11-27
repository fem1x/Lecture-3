using _Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public sealed class ToggleSfxView : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Toggle _toggle;
        [SerializeField] private string _onSfx = "CheckboxOn";
        [SerializeField] private string _offSfx = "CheckboxOff";
        [SerializeField] private string _hoverSfx = "ButtonHover";
        [SerializeField] private string _disabledClickSfx = "ButtonDisabledClick";

        private ISfxPlayer _sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;

        private void OnEnable() => _toggle.onValueChanged.AddListener(HandleValueChanged);

        private void OnDisable() => _toggle.onValueChanged.RemoveListener(HandleValueChanged);

        private void HandleValueChanged(bool isOn) => _sfxPlayer.PlayUI(isOn ? _onSfx : _offSfx, (RectTransform)_toggle.transform);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_toggle.IsInteractable())
                _sfxPlayer.PlayUI(_hoverSfx, (RectTransform)_toggle.transform);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && !_toggle.IsInteractable())
                _sfxPlayer.PlayUI(_disabledClickSfx, (RectTransform)_toggle.transform);
        }
    }
}
