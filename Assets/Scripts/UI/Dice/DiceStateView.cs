using System;
using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class DiceStateView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Views")]
        [SerializeField] private DurabilityView _durabilityView;
        [SerializeField] private DiceDamageView _damageView;

        [Header("Face & Animation")]
        [SerializeField] private Image _diceImage;
        [SerializeField] private Sprite _unrolledSprite;
        [SerializeField] private List<Sprite> _diceSprites = new();
        [SerializeField] private float _flipDuration = 0.4f;

        [Header("Selection")]
        [SerializeField] private Image _border;
        [SerializeField] private Color _selectedBorderColor;
        [SerializeField] private Button _inspectButton;
        [SerializeField] private float _clickAccentScale = 0.12f;
        [SerializeField] private float _clickAccentDuration = 0.22f;

        private Color _defaultBorderColor;
        private Dice _dice;
        private IReadOnlyDiceData _data;
        private ISfxPlayer _sfxPlayer;
        private Tween _flipTween;
        private Tween _clickAccentTween;
        private DiceTableController _tableController;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer) => _sfxPlayer = sfxPlayer;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _defaultBorderColor = _border.color;
            if (_inspectButton != null)
                _inspectButton.onClick.AddListener(ToggleSelection);
        }

        public void Bind(Dice dice, DiceTableController tableController)
        {
            _dice = dice;
            _tableController = tableController;
            var data = dice.Data;
            _data = data;

            _data.OnDataChanged += RefreshAll;
            _data.OnLockChanged += RefreshBorder;
            _data.OnValueChanged += RefreshDiceFace;
            _tableController.OnSelectionAvailabilityChanged += RefreshButtonState;
            _tableController.OnSelectionChanged += RefreshButtonState;

            RefreshAll();
            RefreshBorder(_data.IsLocked);
            _diceImage.sprite = GetFaceSprite(_data.RolledValue);
            RefreshButtonState();
        }

        private void OnDestroy()
        {
            _flipTween?.Kill();
            _clickAccentTween?.Kill();

            if (_data == null) return;
            _data.OnDataChanged -= RefreshAll;
            _data.OnLockChanged -= RefreshBorder;
            _data.OnValueChanged -= RefreshDiceFace;
            _tableController.OnSelectionAvailabilityChanged -= RefreshButtonState;
            _tableController.OnSelectionChanged -= RefreshButtonState;
        }

        private void RefreshAll()
        {
            _damageView.SetDamage(_data.IsDamaged, _data.DamageLevel);
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);
        }

        private void RefreshBorder(bool isLocked) =>
            _border.color = isLocked ? _selectedBorderColor : _defaultBorderColor;

        private void ToggleSelection()
        {
            if (!_dice.TryToggleLock()) return;

            _clickAccentTween?.Kill();
            _clickAccentTween = _diceImage.transform.DOPunchScale(
                Vector3.one * _clickAccentScale,
                _clickAccentDuration,
                vibrato: 5,
                elasticity: 0.65f)
                .SetLink(gameObject);
        }

        private void RefreshButtonState() =>
            _inspectButton.interactable = _dice.gameObject.activeInHierarchy && _dice.CanBeSelected;

        private void RefreshDiceFace(int oldValue, int newValue)
        {
            if (oldValue == newValue) return;

            _flipTween?.Kill();
            _sfxPlayer.Play("DiceFaceFlip");

            _flipTween = _diceImage.transform.DoFlipX(
                onHalfway: () => _diceImage.sprite = GetFaceSprite(newValue),
                time: _flipDuration
            );
        }

        private Sprite GetFaceSprite(int value) =>
            value == -1 ? _unrolledSprite : _diceSprites[value];
    }
}
