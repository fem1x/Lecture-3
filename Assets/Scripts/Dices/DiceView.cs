using System.Threading;
using _Scripts.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Dices
{
    public class DiceView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        public Vector3 InitialScale { get; private set; }
        
        [Header("Faces")] 
        [SerializeField] private DiceFaceView[] _faceViews;
        
        [Header("Drop Settings")]
        [SerializeField] private bool _usePhysicsDrop = true;

        [Header("LiftUp Animation")]
        [SerializeField] private float _liftHeight = 1.2f;
        [SerializeField] private float _liftTime = 0.25f;
        [SerializeField] private float _dropTime = 0.4f;
        [SerializeField] private Ease _liftEase = Ease.OutBack;
        [SerializeField] private Ease _dropEase = Ease.OutBack;

        [Header("References")]
        [SerializeField] private FreeOutline _outline;
        [SerializeField] private Rigidbody _rb;
        
        private IReadOnlyDiceData _data;
        private Tween _moveTween;
        private Sequence _collectSequence;
        private float _floorY = -999f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            InitialScale = transform.localScale;
        }

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnLockChanged += SetLockedVisual;
            _data.OnDataChanged += UpdateFaces;
            
            _outline.SetOutline(_data.IsLocked);
            UpdateFaces();
        }
        
        private void OnDestroy()
        {
            _moveTween?.Kill();
            _collectSequence?.Kill();
            
            if (_data == null) return;
            _data.OnLockChanged -= SetLockedVisual;
            _data.OnDataChanged -= UpdateFaces;
        }
        
        private void UpdateFaces()
        {
            if (_data == null) return;

            for (int i = 0; i < _faceViews.Length; i++)
            {
                int value = _data.GetFaceValue(i);
                _faceViews[i].SetValue(value);
                _faceViews[i].SetCracksLevel(_data.CurrentDurability, _data.MaxDurability);
            }
        }

        private void SetLockedVisual(bool isLocked)
        {
            _outline.SetOutline(isLocked);
            _moveTween?.Kill();
    
            if (isLocked)
            {
                if (_floorY == -999f)
                    _floorY = _rb.position.y;
            
                DoLiftUpTween();
            }
            else
            {
                if (_floorY == -999f) return;
                DoDrop();
            }
        }
        
        public void ResetVisual()
        {
            _moveTween?.Kill();
            transform.DOKill();
            transform.localScale = InitialScale;

            _outline.SetOutline(false);
            _floorY = -999f;
            _rb.isKinematic = true;
        }

        private void DoLiftUpTween()
        {
            _rb.isKinematic = true;
            _moveTween = _rb.DOMoveY(_floorY + _liftHeight, _liftTime).SetEase(_liftEase);
        }

        private void DoDrop()
        {
            if (_usePhysicsDrop)
            {
                _rb.isKinematic = false;
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                return;
            }
            DoDropTween();
        }
        
        private void DoDropTween()
        {
            _moveTween = transform.DOMoveY(_floorY, _dropTime).SetEase(_dropEase)
                .OnComplete(() =>
                {
                    _rb.isKinematic = false;
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                });
        }
    }
}