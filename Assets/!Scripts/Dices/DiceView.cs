using System;
using _Scripts.Interfaces;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Dices
{
    [RequireComponent(typeof(Rigidbody), typeof(Outline))]
    public class DiceView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [Header("Faces")] 
        [SerializeField] private SpriteRenderer[] _faceRenderers;
        [SerializeField] private Sprite[] _faceSprites;
        
        [Header("LiftUp Animation")]
        [SerializeField] private float _liftHeight = 1.2f;
        [SerializeField] private float _liftTime = 0.25f;
        [SerializeField] private Ease _liftEase = Ease.OutBack;
        
        private IReadOnlyDiceData _data;

        private Outline _outline;
        private Rigidbody _rb;
        private Tween _moveTween;
        private float _floorY = -999f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnLockChanged += SetLockedVisual;
            _data.OnDataChanged += UpdateFaces;
            
            _outline.enabled = _data.IsLocked;
            UpdateFaces();
        }
        
        private void OnDestroy()
        {
            _moveTween?.Kill();
            
            if (_data == null) return;
            _data.OnLockChanged -= SetLockedVisual;
            _data.OnDataChanged -= UpdateFaces;
        }
        
        public void UpdateFaces()
        {
            if (_data == null) return;

            for (int i = 0; i < _faceRenderers.Length; i++)
            {
                int value = _data.GetFaceValue(i);
                _faceRenderers[i].sprite = _faceSprites[value];
            }
        }

        public void SetLockedVisual(bool isLocked)
        {
            _outline.enabled = isLocked;
            bool isAnimating = _moveTween != null && _moveTween.IsActive();
            _moveTween?.Kill();
            
            if (isLocked)
            {
                if (!isAnimating || _floorY == -999f)
                    _floorY = _rb.position.y;
                DoLiftUpTween();
            }
            else
            {
                if (_floorY == -999f) return;
                DoDropTween();
            }
        }
        
        public void ResetVisual()
        {
            _moveTween?.Kill();
            _outline.enabled = false;
            _floorY = -999f;
        }

        private void DoLiftUpTween()
        {
            _rb.isKinematic = true;
            _moveTween = _rb.DOMoveY(_floorY + _liftHeight, _liftTime).SetEase(_liftEase);
        }
        
        private void DoDropTween()
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _floorY = -999f;
        }
    }
}