using System;
using System.Collections;
using _Scripts.Configs;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.Dices
{
    public class Dice : MonoBehaviour, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [field: SerializeField] public DiceView View { get; private set; }
        public DiceData Data { get; private set; }
        
        [Header("References")]
        [SerializeField] private Rigidbody _rb;
        
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
        
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, CameraShaker cameraShaker, DiceTableController tableController)
        {
            _sfxPlayer = sfxPlayer;
            _cameraShaker = cameraShaker;
            _tableController = tableController;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public bool IsStopped => _rb.isKinematic || 
                                 (_rb.linearVelocity.sqrMagnitude < 0.01f && _rb.angularVelocity.sqrMagnitude < 0.01f);

        public bool IsRolled => Data.RolledValue != -1;
        public bool CanBeSelected => IsRolled && !_tableController.IsInteractionLocked;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Init(DiceData data)
        {
            Data = data;
            View.Bind(data);
        }

        #region Locking
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!TryToggleLock()) return;
            
            if (Data.IsLocked)
                _sfxPlayer.Play(_lockSfx, transform.position);
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
            Data?.RoundReset();
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
                Data.SetRolledValue(Data.GetFaceValue(bestFaceIndex));
            
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            var contactPoint = collision.contacts.Length > 0 
                ? collision.contacts[0].point 
                : transform.position;
            
            _sfxPlayer.Play(_hitSfx, contactPoint);
        }
    }
}
