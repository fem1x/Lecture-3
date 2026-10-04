using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.Dices
{
    [RequireComponent(typeof(Rigidbody), typeof(DiceView))]
    public class Dice : MonoBehaviour, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [field: SerializeField] public DiceFace[] Faces { get; private set; } = 
        {
            new(1, Vector3.up),
            new(6, Vector3.down),
            new(5, Vector3.right),
            new(2, Vector3.left),
            new(3, Vector3.forward),
            new(4, Vector3.back)
        };
        
        [Header("Durability")]
        [field: SerializeField] public int MaxDurability { get; private set; } = 2;
        public int CurrentDurability { get; private set; }
        
        [Header("SFX")]
        [SerializeField] private string _lockSfx;
        [SerializeField] private string _throwSfx;
        [SerializeField] private string _hitSfx;
        
        private Rigidbody _rb;
        private DiceView _view;
        private Coroutine _stopCoroutine;
        private ISfxPlayer _sfxPlayer;
        
        [Inject]
        public void Construct(ISfxPlayer sfxPlayer)
        {
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public int Value { get; private set; } = -1;
        public bool IsLocked { get; private set; }
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
            CurrentDurability = MaxDurability;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsStopped) return;
            ToggleLock();
        }

        public void SetLock(bool isLocked)
        {
            if (isLocked == IsLocked) return;
            ToggleLock();
        }
        
        private void ToggleLock()
        {
            IsLocked = !IsLocked;
            _view.SetLockedVisual(IsLocked);
            
            if(IsLocked) _sfxPlayer.Play(_lockSfx, transform.position); //SFX
        }

        public void Reset()
        {
            Value = -1;
            IsLocked = false;
            _view.ResetVisual();
            
            if (_rb.isKinematic) return;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            _rb.position = position;
            _rb.rotation = rotation;
        }
        
        public void Roll(Vector3 force, Vector3 torque)
        {
            _rb.isKinematic = false;
            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(torque, ForceMode.Impulse);
            
            _sfxPlayer.Play(_throwSfx, transform.position); //SFX
        }
        
        #region Face Values
        public void TakeHit(int amount = 1)
        {
            CurrentDurability -= amount;

            if (CurrentDurability <= 0)
            {
                ReduceAllValues(1);
                CurrentDurability = MaxDurability;
            }
        }

        public void ReduceAllValues(int amount = 1)
        {
            foreach (var face in Faces)
                face.ReduceValue(amount);
            
            UpdateValue();
            _view.UpdateFaceRenderers(Faces);
        }

        public void ResetAllValues()
        {
            foreach (var face in Faces)
                face.Reset();
        }
        #endregion

        public void UpdateValue()
        {
            DiceFace bestFace = null;
            float maxDot = -1f;

            for (int i = 0; i < Faces.Length; i++)
            {
                Vector3 worldDirection = transform.TransformDirection(Faces[i].Direction);
                float dot = Vector3.Dot(worldDirection, Vector3.up);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestFace = Faces[i];
                }
            }
            Value = bestFace.CurrentValue;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Vector3 contactPoint = collision.contacts.Length > 0 
                ? collision.contacts[0].point 
                : transform.position;
            
            _sfxPlayer.Play(_hitSfx, contactPoint);
        }
    }
}
