using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Scripts.Dices
{
    [RequireComponent(typeof(Rigidbody), typeof(DiceView))]
    public class Dice : MonoBehaviour, IPointerClickHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [SerializeField] 
        private DiceFace[] _faces = 
        {
            new(1, Vector3.up),
            new(6, Vector3.down),
            new(5, Vector3.right),
            new(2, Vector3.left),
            new(3, Vector3.forward),
            new(4, Vector3.back)
        };
        
        [Header("Durability")]
        [SerializeField] private int _maxDurability = 2;
        
        private int _currentDurability;
        private Rigidbody _rb;
        private DiceView _view;
        private Coroutine _stopCoroutine;
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
            _currentDurability = _maxDurability;
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
        }
        
        #region Face Values
        public void TakeHit(int amount = 1)
        {
            _currentDurability -= amount;

            if (_currentDurability <= 0)
            {
                ReduceAllValues(1);
                _currentDurability = _maxDurability;
            }
        }

        public void ReduceAllValues(int amount = 1)
        {
            foreach (var face in _faces)
                face.ReduceValue(amount);
            
            UpdateValue();
            _view.UpdateFaceRenderers(_faces);
        }

        public void ResetAllValues()
        {
            foreach (var face in _faces)
                face.Reset();
        }
        #endregion

        public void UpdateValue()
        {
            DiceFace bestFace = null;
            float maxDot = -1f;

            for (int i = 0; i < _faces.Length; i++)
            {
                Vector3 worldDirection = transform.TransformDirection(_faces[i].Direction);
                float dot = Vector3.Dot(worldDirection, Vector3.up);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestFace = _faces[i];
                }
            }
            Value = bestFace.CurrentValue;
            
            Debug.Log($"[{gameObject.name}] Top face Direction: {bestFace.Direction}, Value: {Value}");
        }
    }
}
