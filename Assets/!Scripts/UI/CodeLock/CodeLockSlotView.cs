using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.CodeLock
{
    public class CodeLockSlotView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [field: SerializeField] public Image DiceImage { get; private set; }
        [SerializeField] private List<Sprite> _diceSprites;
        
        [Header("Active / Inactive")]
        [SerializeField] private Image _activeOverlayImage;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => RemoveValue();
        
        public void InsertValue(int value)
        {
            DiceImage.sprite = _diceSprites[value];
            DiceImage.gameObject.SetActive(true);
        }

        public void RemoveValue()
        {
            DiceImage.sprite = null;
            DiceImage.gameObject.SetActive(false);
        }

        public void SetActiveState(bool isActive)
        {
            _activeOverlayImage.gameObject.SetActive(isActive);
        }
    }
}