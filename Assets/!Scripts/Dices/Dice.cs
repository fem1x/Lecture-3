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
        private readonly (int Value, Vector3 Direction)[] _faces = 
        {
            (1, Vector3.up),
            (6, Vector3.down),
            (5, Vector3.right),
            (2, Vector3.left),
            (3, Vector3.forward),
            (4, Vector3.back)
        };
    
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
        
        public void Roll(Vector3 force, Vector3 torque)
        {
            _rb.isKinematic = false;
            _rb.AddForce(force, ForceMode.Impulse);
            _rb.AddTorque(torque, ForceMode.Impulse);
        
            if (_stopCoroutine != null)
                StopCoroutine(_stopCoroutine);
            _stopCoroutine = StartCoroutine(Co_WaitUntilStopped());
        }
    
        private IEnumerator Co_WaitUntilStopped()
        {
            yield return new WaitForSeconds(0.2f);
            yield return new WaitUntil(() => IsStopped);

            UpdateValue();
            _stopCoroutine = null;
        }

        public void UpdateValue()
        {
            int bestValue = 1;
            float maxDot = -1f;

            for (int i = 0; i < _faces.Length; i++)
            {
                Vector3 worldDirection = transform.TransformDirection(_faces[i].Direction);
                float dot = Vector3.Dot(worldDirection, Vector3.up);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    bestValue = _faces[i].Value;
                }
            }
            Value = bestValue;
        }
    }
}
