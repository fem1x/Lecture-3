using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DiceRepairView : MonoBehaviour
    {
        [SerializeField] private Button _repairButton;

        public void SetRepairMode(bool enable)
        {
            _repairButton.interactable = enable;
            _repairButton.gameObject.SetActive(enable);
        }
    }
}