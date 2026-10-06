using System.Collections.Generic;
using _Scripts.Interfaces;
using DG.Tweening;
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
        
        [Header("Flip Animation")]
        [SerializeField] private float _flipDuration = 0.2f;
        [SerializeField] private Ease _flipEaseIn = Ease.InQuad;
        [SerializeField] private Ease _flipEaseOut = Ease.OutBack;
        private Tween _flipTween;
        
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
        
        [Header("Repair")]
        [SerializeField] private DiceRepairView _repairView;
        
        private IReadOnlyDiceData _data;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void SetRepairMode(bool enable) => _repairView.SetRepairMode(enable);
        
        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _repairView.Bind(data);
            
            _data.OnDataChanged += RefreshAll;
            _data.OnLockChanged += RefreshBorder;
            _data.OnValueChanged += RefreshDiceFace;
            
            _defaultBorderColor = _border.color;
            RefreshAll();
            RefreshBorder(_data.IsLocked);
            SetInitialFace();
        }
        
        private void OnDestroy()
        {
            if (_data == null) return;
            _data.OnDataChanged -= RefreshAll;
            _data.OnLockChanged -= RefreshBorder;
            _data.OnValueChanged -= RefreshDiceFace;
        }

        private void RefreshAll()
        {
            RefreshDamage();
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

        private void RefreshDurability()
        {
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);
        }
        
        private void RefreshDiceFace(int oldValue, int newValue)
        {
            if (oldValue == newValue) return;
            _flipTween?.Kill();

            var nextSprite = GetFaceSprite(newValue);
            var halfTime = _flipDuration * 0.5f;

            _flipTween = _diceImage.rectTransform.DOScaleX(0f, halfTime)
                .SetEase(_flipEaseIn)
                .OnComplete(() =>
                {
                    _diceImage.sprite = nextSprite;
                    _flipTween = _diceImage.rectTransform.DOScaleX(1f, halfTime).SetEase(_flipEaseOut);
                });
        }

        private void SetInitialFace()
        {
            _diceImage.sprite = GetFaceSprite(_data.RolledValue);
        }

        private Sprite GetFaceSprite(int value)
        {
            return value == -1 ? _unrolledSprite : _diceSprites[value];
        }
    }
}