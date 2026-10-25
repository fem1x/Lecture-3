using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.CodeLock
{
    public class CodeLockSlotView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [SerializeField] private Image _diceImage;
        [SerializeField] private List<Sprite> _diceSprites;
        
        [Header("Active / Inactive")]
        [SerializeField] private Image _activeOverlayImage;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => RemoveValue();
        
        public void InsertValue(int value)
        {
            _diceImage.sprite = _diceSprites[value];
            _diceImage.gameObject.SetActive(true);
        }

        public void RemoveValue()
        {
            _diceImage.sprite = null;
            _diceImage.gameObject.SetActive(false);
        }

        public void SetActiveState(bool isActive)
        {
            _activeOverlayImage.gameObject.SetActive(isActive);
        }
    }
}