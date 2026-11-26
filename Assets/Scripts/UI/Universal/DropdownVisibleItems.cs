using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class DropdownVisibleItems : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField, Min(1)] private int _maxVisibleItems = 5;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            var template = GetComponent<TMP_Dropdown>().template;
            var item = (RectTransform)template.GetComponentInChildren<Toggle>(true).transform;
            var content = (RectTransform)item.parent;
            var itemHeight = item.rect.height;
            var contentPadding = content.rect.height - itemHeight;
            template.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                itemHeight * Mathf.Max(1, _maxVisibleItems) + contentPadding);
        }
    }
}
