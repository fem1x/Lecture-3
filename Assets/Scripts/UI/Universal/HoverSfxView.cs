using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.UI
{
    [DisallowMultipleComponent]
    public sealed class HoverSfxView : MonoBehaviour, IPointerEnterHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private string _hoverSfx = "ButtonHover";

        private ISfxPlayer _sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;

        public void OnPointerEnter(PointerEventData eventData) => _sfxPlayer.PlayUI(_hoverSfx, (RectTransform)transform);
    }
}
