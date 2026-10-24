using _Scripts.Charms;
using _Scripts.UI.Charms;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.UI.Tooltips
{
    public class CharmTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [field: SerializeField] public RectTransform TargetRect { get; private set; }
        [field: SerializeField] public CharmEquippedView EquippedView { get; private set; }

        private CharmTooltipView _tooltipView;

        #region DI
        [Inject]
        public void Construct(CharmTooltipView tooltipView) => _tooltipView = tooltipView;
        #endregion

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnDisable() => _tooltipView.HideImmediate();

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void OnPointerEnter(PointerEventData eventData)
        {
            var config = EquippedView.CharmGeneralView.Config;
            _tooltipView.Show(TargetRect, config.Name.GetLocalizedString(), config.GetDescription());
        }

        public void OnPointerExit(PointerEventData eventData) => _tooltipView.Hide();
    }
}