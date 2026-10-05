using System.Collections.Generic;
using _Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DiceStateView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Dice Face")] 
        [SerializeField] private Image _diceImage;
        [SerializeField] private Sprite _unrolledSprite;
        [SerializeField] private List<Sprite> _diceSprites;
        
        [Header("Damage Level")]
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private Image _damageImage;
        [SerializeField] private Color _damagedColor;
        [SerializeField] private Color _notDamagedColor;
        
        [Header("Durability")]
        [SerializeField] DurabilityView _durabilityView;
        
        private IReadOnlyDiceData _data;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnDataChanged += Refresh;
            
            Refresh();
        }
        
        private void OnDestroy() => _data.OnDataChanged -= Refresh;

        private void Refresh()
        {
            RefreshDamage();
            RefreshDiceFace();
            RefreshDurability();
        }

        private void RefreshDamage()
        {
            if (_data.IsDamaged)
            {
                _damageText.text = $"-{_data.DamageLevel.ToString()}";
                _damageImage.color = _damagedColor;
            }
            else
            {
                _damageText.text = "OK";
                _damageImage.color = _notDamagedColor;
            }
        }

        private void RefreshDiceFace()
        {
            var sprite = _data.RolledValue == -1 ? _unrolledSprite : _diceSprites[_data.RolledValue];
            _diceImage.sprite = sprite;
        }

        private void RefreshDurability()
        {
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);
        }
    }
}