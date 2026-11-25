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
        [SerializeField] private string _hoverSfx = "ButtonHover";
        [SerializeField] private string _disabledClickSfx = "ButtonDisabledClick";

        private ISfxPlayer _sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_toggle.IsInteractable())
                _sfxPlayer.Play(_hoverSfx);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && !_toggle.IsInteractable())
                _sfxPlayer.Play(_disabledClickSfx);
        }
    }
}
