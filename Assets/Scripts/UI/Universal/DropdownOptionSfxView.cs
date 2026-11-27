using _Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public sealed class DropdownOptionSfxView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private Toggle _toggle;
        private ISfxPlayer _sfxPlayer;
        private string _hoverSfx;
        private string _clickSfx;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        internal void Initialize(Toggle toggle, ISfxPlayer sfxPlayer, string hoverSfx, string clickSfx)
        {
            _toggle = toggle;
            _sfxPlayer = sfxPlayer;
            _hoverSfx = hoverSfx;
            _clickSfx = clickSfx;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_toggle.IsInteractable())
                _sfxPlayer.PlayUI(_hoverSfx, (RectTransform)_toggle.transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                PlayClick();
        }

        public void OnSubmit(BaseEventData eventData) => PlayClick();

        private void PlayClick()
        {
            // TMP begins closing the list inside Toggle's event, before this handler runs.
            if (_toggle.interactable)
                _sfxPlayer.PlayUI(_clickSfx, (RectTransform)_toggle.transform);
        }
    }
}
