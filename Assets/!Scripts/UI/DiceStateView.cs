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
        [Space]
        [SerializeField] private Sprite _unrolledSprite;
        [SerializeField] private List<Sprite> _diceSprites;
        
        [Header("Damage Level")]
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private Image _damageIndicator;
        [Space]
        [SerializeField] private Color _damagedIndicatorColor;
        [SerializeField] private Color _damagedTextColor;
        [Space(0.5f)]
        [SerializeField] private Color _notDamagedIndicatorColor;
        [SerializeField] private Color _notDamagedTextColor;
        [Space] 
        [SerializeField] private bool _hideUndamagedIndicator = true;
        
        [Header("Border")]
        [SerializeField] private Image _border;
        [SerializeField] private Color _selectedBorderColor;
        private Color _defaultBorderColor;
        
        [Header("Durability")]
        [SerializeField] DurabilityView _durabilityView;
        
        private IReadOnlyDiceData _data;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnDataChanged += RefreshAll;
            _data.OnLockChanged += RefreshBorder;
            
            _defaultBorderColor = _border.color;
            RefreshAll();
        }
        
        private void OnDestroy()
        {
            if (_data == null) return;
            _data.OnDataChanged -= RefreshAll;
            _data.OnLockChanged -= RefreshBorder;
        }

        private void RefreshAll()
        {
            RefreshDamage();
            RefreshDiceFace();
            
            if(_durabilityView != null)
                RefreshDurability();
        }

        private void RefreshBorder(bool isLocked)
        {
            _border.color = isLocked ? _selectedBorderColor : _defaultBorderColor;
        }
        
        private void RefreshDamage()
        {
            if (_hideUndamagedIndicator)
            {
                _damageIndicator.gameObject.SetActive(_data.IsDamaged);
                if (!_data.IsDamaged) return;
            }
            
            if (_data.IsDamaged)
            {
                _damageText.text = $"-{_data.DamageLevel.ToString()}";
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