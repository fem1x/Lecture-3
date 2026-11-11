using System;
using System.Collections.Generic;
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
        public event Action<IReadOnlyDiceData> OnInspectRequested;

        [Header("Views")]
        [SerializeField] private DurabilityView _durabilityView;
        [SerializeField] private DiceRepairView _repairView;
        [SerializeField] private DiceDamageView _damageView;

        [Header("Face & Animation")]
        [SerializeField] private Image _diceImage;
        [SerializeField] private Sprite _unrolledSprite;
        [SerializeField] private List<Sprite> _diceSprites;
        [SerializeField] private float _flipDuration = 0.4f;

        [Header("Selection")]
        [SerializeField] private Image _border;
        [SerializeField] private Color _selectedBorderColor;
        [SerializeField] private Button _inspectButton;

        private Color _defaultBorderColor;
        private IReadOnlyDiceData _data;
        private ISfxPlayer _sfxPlayer;
        private Tween _flipTween;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer)
        {
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _defaultBorderColor = _border.color;
            if (_inspectButton != null)
                _inspectButton.onClick.AddListener(() => OnInspectRequested?.Invoke(_data));
        }

        public void SetRepairMode(bool enable) => _repairView.SetRepairMode(enable);

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _repairView.Bind(data);

            _data.OnDataChanged += RefreshAll;
            _data.OnLockChanged += RefreshBorder;
            _data.OnValueChanged += RefreshDiceFace;

            RefreshAll();
            RefreshBorder(_data.IsLocked);
            _diceImage.sprite = GetFaceSprite(_data.RolledValue);
        }

        private void OnDestroy()
        {
            _flipTween?.Kill();

            if (_data == null) return;
            _data.OnDataChanged -= RefreshAll;
            _data.OnLockChanged -= RefreshBorder;
            _data.OnValueChanged -= RefreshDiceFace;
        }

        private void RefreshAll()
        {
            _damageView.SetDamage(_data.IsDamaged, _data.DamageLevel);
            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);
        }

        private void RefreshBorder(bool isLocked) => 
            _border.color = isLocked ? _selectedBorderColor : _defaultBorderColor;

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