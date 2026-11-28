
using System;
using _Scripts.Configs;
using _Scripts.Managers;
using _Scripts.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.Dices
{
    public class Dice : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public DiceView View { get; private set; }
        public DiceData Data { get; private set; }
        public event Action<Dice, Collider> RollCollisionEntered;

        [Header("References")]
        [SerializeField] private Rigidbody _rb;

        [Header("Dice Tooltips")]
        [SerializeField] private DiceTooltipView _diceTooltipPrefab;
        [SerializeField] private TMPro.TMP_Text _damageIndicatorPrefab;

        [Header("Camera Shake")]
        [SerializeField] private CameraShakePreset _rollShake;

        [Header("SFX")]
        [SerializeField] private string _lockSfx;
        [SerializeField] private string _throwSfx;
        [SerializeField] private string _hitSfx;

        #region DI
        private ISfxPlayer _sfxPlayer;
        private CameraShaker _cameraShaker;
        private DiceTableController _tableController;
        private DiceHoverTooltip _hoverTooltip;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, CameraShaker cameraShaker, DiceTableController tableController, DiceHoverTooltip hoverTooltip)
        {
            _sfxPlayer = sfxPlayer;
            _cameraShaker = cameraShaker;
            _tableController = tableController;
            _hoverTooltip = hoverTooltip;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public bool IsStopped => _rb.isKinematic ||
                                 (_rb.linearVelocity.sqrMagnitude < 0.001f && _rb.angularVelocity.sqrMagnitude < 0.001f);

        public bool IsRolled => Data.RolledValue != -1;
        public bool CanBeSelected => IsRolled && !_isRolling && !_tableController.IsInteractionLocked;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Init(DiceData data)
        {
            Data = data;
            View.Bind(data);
            _tooltipCollider = GetComponentInChildren<Collider>();
            _tooltip = _hoverTooltip.Create(_diceTooltipPrefab, data.Id);
            _damageIndicator = _hoverTooltip.CreateDamageIndicator(_damageIndicatorPrefab, data.Id);
        }

        internal DiceTooltipView Tooltip => _tooltip;
        internal DiceDamageIndicatorView DamageIndicator => _damageIndicator;
        internal Bounds TooltipAnchorBounds => _tooltipCollider.bounds;
        internal bool ShouldShowTooltip => IsRolled && !_isRolling && !_isReturning &&
            (_hoverTooltip.AutoShow || (_isPointerOver && CanBeSelected));

        private DiceTooltipView _tooltip;
        private DiceDamageIndicatorView _damageIndicator;
        private Collider _tooltipCollider;
        private bool _isPointerOver;
        private bool _isRolling;
        private bool _isReturning;

        public void ShowDamageChange()
        {
            if (ShouldShowTooltip && Data.DamageLevel > 0)
                _damageIndicator.ShowDamageChange(Data.DamageLevel);
        }

        public bool AreTooltipsHidden => !_tooltip.gameObject.activeSelf && !_damageIndicator.gameObject.activeSelf;

        public void BeginReturn()
        {
            _isReturning = true;
            _tooltip.SetVisible(false);
            _damageIndicator.SetVisible(false);
        }

        public void CancelReturn() => _isReturning = false;

        private void Update()
        {
            View.SetHovered(_isPointerOver && CanBeSelected);
        }

        private void OnDisable()
        {
            _isPointerOver = false;
            View.SetHovered(false);
            if (_tooltip != null) _tooltip.HideImmediately();
            if (_damageIndicator != null) _damageIndicator.HideImmediately();
        }

        private void OnDestroy()
        {
            _hoverTooltip?.Remove(this);
            if (_tooltip != null) Destroy(_tooltip.gameObject);
            if (_damageIndicator != null) Destroy(_damageIndicator.gameObject);
        }

        public void BeginRoll()
        {
            _isRolling = true;
            _tableController.NotifySelectionAvailabilityChanged();
            View.SetHovered(false);
            _tooltip.SetVisible(false);
            _damageIndicator.SetVisible(false);
        }

        internal void EndRoll()
        {
            _isRolling = false;
            _tableController.NotifySelectionAvailabilityChanged();
        }

        public void OnPointerEnter(PointerEventData eventData) => _isPointerOver = true;

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerOver = false;
            View.SetHovered(false);
            if (!_hoverTooltip.AutoShow)
            {
                _tooltip.SetVisible(false);
                _damageIndicator.SetVisible(false);
            }
        }

        #region Locking
        public void OnPointerClick(PointerEventData eventData)
        {
            TryToggleLock();
        }

        public void SetLock(bool isLocked)
        {
            if (!CanBeSelected) return;
            Data.SetLock(isLocked);
        }

        public bool TryToggleLock()
        {
            if (!CanBeSelected) return false;
            Data.ToggleLock();

            if (Data.IsLocked)
                _sfxPlayer.Play(_lockSfx, transform.position);

            return true;
        }
        #endregion

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            _rb.position = position;
            _rb.rotation = rotation;
            transform.position = position;
            transform.rotation = rotation;

            Physics.SyncTransforms();
        }

        public void PrepareForRoll()
        {
            _isRolling = false;
            _isReturning = false;
            Data.RoundReset();
            _tooltip.SetVisible(false);
            _damageIndicator.SetVisible(false);
            View.ResetVisual();

            if (!_rb.isKinematic)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }

            _rb.isKinematic = true;
        }

        public void Roll(Vector3 force, Vector3 torque)
        {
            _rb.constraints = RigidbodyConstraints.None;
            _rb.isKinematic = false;

            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(torque, ForceMode.Impulse);

            
            //FX
            _sfxPlayer.Play(_throwSfx, transform.position);
            _cameraShaker.DoShake(_rollShake);
        }

        public void UpdateValue()
        {
            var bestFaceIndex = -1;
            var maxDot = -1f;

            for (int i = 0; i < 6; i++)
            {
                var worldDirection = transform.TransformDirection(Data.GetFaceDirection(i));
                var dot = Vector3.Dot(worldDirection, Vector3.up);

                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestFaceIndex = i;
                }
            }

            if (bestFaceIndex >= 0)
                Data.SetRolledFace(bestFaceIndex);

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isRolling && !_rb.isKinematic)
                RollCollisionEntered?.Invoke(this, collision.collider);

            var contactPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : transform.position;

            _sfxPlayer.Play(_hitSfx, contactPoint);
        }
    }
}
