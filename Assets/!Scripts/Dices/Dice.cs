using System;
using System.Collections;
using _Scripts.Configs;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.Dices
{
    [RequireComponent(typeof(Rigidbody), typeof(DiceView))]
    public class Dice : MonoBehaviour, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public DiceData Data { get; private set; }
        
        [Header("Camera Shake")]
        [SerializeField] private CameraShakePreset _rollShake;
        
        [Header("SFX")]
        [SerializeField] private string _lockSfx;
        [SerializeField] private string _throwSfx;
        [SerializeField] private string _hitSfx;
        
        private Rigidbody _rb;
        private DiceView _view;
        
        #region DI
        private ISfxPlayer _sfxPlayer;
        private CameraShaker _cameraShaker;
        
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer, CameraShaker cameraShaker)
        {
            _sfxPlayer = sfxPlayer;
            _cameraShaker = cameraShaker;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public bool IsStopped
        {
            get
            {
                if (this == null || _rb == null) return true;

                return _rb.isKinematic || 
                       (_rb.linearVelocity.sqrMagnitude < 0.01f && _rb.angularVelocity.sqrMagnitude < 0.01f);
            }
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
    
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _view = GetComponent<DiceView>();
        }

        public void Init(DiceData data)
        {
            Data = data;
            _view.Bind(data);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsStopped) return;
            Data.ToggleLock();
            
            if (Data.IsLocked)
                _sfxPlayer.Play(_lockSfx, transform.position);
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            _rb.MovePosition(position);
            _rb.MoveRotation(rotation);
        }
        
        public void PrepareForRoll()
        {
            Data?.RoundReset();
            _view.ResetVisual();
            
            if (!_rb.isKinematic)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
            
            _rb.isKinematic = true;
        }
        
        public void Roll(Vector3 force, Vector3 torque)
        {
            _rb.isKinematic = false;
            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(torque, ForceMode.Impulse);
            
            //FX
            _sfxPlayer.Play(_throwSfx, transform.position); 
            _cameraShaker.Shake(_rollShake);
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
            {
                Data.SetRolledValue(Data.GetFaceValue(bestFaceIndex));
            }
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
