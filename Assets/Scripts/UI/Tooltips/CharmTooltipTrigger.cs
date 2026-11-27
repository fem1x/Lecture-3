using _Scripts.UI.Charms;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    public class CharmTooltipTrigger : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        [field: SerializeField] public RectTransform TargetRect { get; private set; }
        [field: SerializeField] public CharmEquippedView EquippedView { get; private set; }
        [SerializeField] private TitledTooltipTrigger _tooltipTrigger;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _tooltipTrigger.ContentProvider = GetContent;

        private void OnDestroy() => _tooltipTrigger.ContentProvider = null;

        private (string title, string text) GetContent()
        {
            var config = EquippedView.CharmGeneralView.Config;
            return (config.Name.GetLocalizedString(), config.GetDescription());
        }
    }
}
