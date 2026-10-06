using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DurabilityView : MonoBehaviour
    {
        [SerializeField] private Image _cracksOverlay;
        
        public void SetDurability(int current, int max)
        {
            var isDamaged = current < max;
            _cracksOverlay.gameObject.SetActive(isDamaged);
        }
    }
}