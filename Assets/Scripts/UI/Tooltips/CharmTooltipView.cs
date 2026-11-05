using TMPro;
using UnityEngine;

namespace _Scripts.UI.Tooltips
{
    public class CharmTooltipView : TooltipViewBase
    {
        [field: SerializeField] public TMP_Text TitleText { get; private set; }
        [field: SerializeField] public TMP_Text DescText { get; private set; }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Show(RectTransform target, string title, string description)
        {
            TitleText.text = title;
            DescText.text = description;

            base.Show(target);
        }
    }
}