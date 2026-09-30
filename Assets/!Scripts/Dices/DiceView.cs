using System;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Dices
{
    [RequireComponent(typeof(Rigidbody), typeof(Outline))]
    public class DiceView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [Header("LiftUp Animation")]
        [SerializeField] private float _liftHeight = 1.2f;
        [SerializeField] private float _liftTime = 0.25f;
        [SerializeField] private float _dropTime = 0.25f;
        [SerializeField] private Ease _liftEase = Ease.OutBack;
        [SerializeField] private Ease _dropEase = Ease.InQuad;
        
        private Outline _outline;
        private Rigidbody _rb;
        private Tween _moveTween;
        private float _floorY;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }
        
        private void OnDestroy()
        {
            _moveTween?.Kill();
        }

        public void SetLockedVisual(bool isLocked)
        {
            _outline.enabled = isLocked;
            _moveTween?.Kill();
            if (isLocked)
            {
                DoLiftUpTween();
            }

            else
            {
                DoDropTween();
            }
        }

        public void DoLiftUpTween()
        {
            _floorY = _rb.position.y;
            _rb.isKinematic = true;
            _moveTween = _rb.DOMoveY(_floorY + _liftHeight, _liftTime).SetEase(_liftEase);
        }
        
        public void DoDropTween()
        {
            _moveTween = _rb.DOMoveY(_floorY, _dropTime).SetEase(_dropEase)
                .OnComplete(() =>
                {
                    _rb.isKinematic = false;
                });
        }
    }
}