using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Utility;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.UI
{
    public class DiceInspectorView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Components")]
        [SerializeField] private DiceModelDisplayView _displayView;
        [SerializeField] private ModelRotator _rotator;
        [SerializeField] private MeshRenderer _backdropRenderer;
        [SerializeField] private ClickTrigger _backdropTrigger;

        [Header("Animation - Fade")]
        [SerializeField] private float _fadeTime = 0.6f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuart;

        [Header("Animation - Scale")]
        [SerializeField] private float _scaleInTime = 0.6f;
        [SerializeField] private Ease _scaleInEase = Ease.OutBack;
        [SerializeField] private float _scaleOutTime = 0.4f;
        [SerializeField] private Ease _scaleOutEase = Ease.OutQuart;

        [Header("Animation - Spin Impulse")]
        [SerializeField] private Vector3[] _spinPresets = 
        {
            new(360f, 720f, 180f),
            new(-720f, 360f, -180f),
            new(720f, -720f, 360f),
            new(0f, 1080f, 360f)
        };
        [SerializeField] private float _spinTime = 0.7f;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        private Quaternion _defaultRotation;
        private Vector3 _defaultScale;
        private Material _backdropMaterial;
        private Color _backdropTargetColor;

        private Sequence _openSequence;
        private Sequence _closeSequence;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _defaultRotation = _displayView.transform.localRotation;
            _defaultScale = _displayView.transform.localScale;

            InitBackdropMaterial();
            _backdropTrigger.Clicked += Close;

            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void OnDestroy()
        {
            KillSequences();
            _backdropTrigger.Clicked -= Close;
        }

        public void Open(IReadOnlyDiceData data)
        {
            KillSequences();

            gameObject.SetActive(true);
            _displayView.Bind(data);

            _rotator.enabled = false;
            PrepareOpenState();

            _openSequence = DOTween.Sequence()
                .Join(_backdropMaterial.DOFade(_backdropTargetColor.a, _fadeTime).SetEase(_fadeEase))
                .Join(_displayView.transform.DoScaleIn(_scaleInTime, _scaleInEase))
                .Join(SpinTween())
                .OnComplete(() => _rotator.enabled = true)
                .SetLink(gameObject);
        }

        public void Close()
        {
            KillSequences();
            _rotator.enabled = false;

            _closeSequence = DOTween.Sequence()
                .Join(_displayView.transform.DoScaleOut(_scaleOutTime, _scaleOutEase))
                .Join(_backdropMaterial.DOFade(0f, _fadeTime).SetEase(_fadeEase))
                .OnComplete(ResetAndDeactivate)
                .SetLink(gameObject);
        }

        private void InitBackdropMaterial()
        {
            _backdropMaterial = _backdropRenderer.material;
            _backdropTargetColor = _backdropMaterial.color;

            var transparentColor = _backdropTargetColor;
            transparentColor.a = 0f;
            _backdropMaterial.color = transparentColor;
        }

        private void PrepareOpenState()
        {
            var startColor = _backdropTargetColor;
            startColor.a = 0f;
            _backdropMaterial.color = startColor;

            _displayView.transform.localScale = _defaultScale;

            var targetEuler = _defaultRotation.eulerAngles;
            var randomAxis = _spinPresets[Random.Range(0, _spinPresets.Length)];
            _displayView.transform.localEulerAngles = targetEuler + randomAxis;
        }

        private Tween SpinTween()
        {
            return _displayView.transform
                .DOLocalRotate(_defaultRotation.eulerAngles, _spinTime, RotateMode.FastBeyond360)
                .SetEase(_spinEase);
        }

        private void ResetAndDeactivate()
        {
            _displayView.transform.localScale = _defaultScale;
            _displayView.transform.localRotation = _defaultRotation;
            gameObject.SetActive(false);
        }

        private void KillSequences()
        {
            _openSequence?.Kill();
            _closeSequence?.Kill();
        }
    }
}