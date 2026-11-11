using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DiceDamageView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private Image _damageIndicator;
        
        [Header("Damaged Colors")]
        [SerializeField] private Color _damagedIndicatorColor;
        [SerializeField] private Color _damagedTextColor;
        
        [Header("Undamaged Colors")]
        [SerializeField] private Color _notDamagedIndicatorColor;
        [SerializeField] private Color _notDamagedTextColor;
        
        [Header("Settings")]
        [SerializeField] private bool _hideUndamagedIndicator = true;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetDamage(bool isDamaged, int damageLevel)
        {
            if (_hideUndamagedIndicator)
            {
                _damageIndicator.gameObject.SetActive(isDamaged);
                if (!isDamaged) return;
            }

            if (isDamaged)
            {
                _damageText.text = $"-{damageLevel.ToString()}";
                _damageText.color = _damagedTextColor;
                _damageIndicator.color = _damagedIndicatorColor;
            }
            else
            {
                _damageText.text = "OK";
                _damageText.color = _notDamagedTextColor;
                _damageIndicator.color = _notDamagedIndicatorColor;
            }
        }
    }
}