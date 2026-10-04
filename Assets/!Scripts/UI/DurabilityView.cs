using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DurabilityView : MonoBehaviour
    {
        [SerializeField] private Image[] _pointImages;

        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _inactiveColor;
        
        public void SetDurability(int current, int max)
        {
            for (int i = 0; i < _pointImages.Length; i++)
            {
                bool isActive = i < current;
                var color = isActive ? _activeColor : _inactiveColor;
                _pointImages[i].color = color;
            }
        }
    }
}