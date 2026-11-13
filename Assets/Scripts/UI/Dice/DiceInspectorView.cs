using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Utility;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DiceInspectorView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Components")]
        [SerializeField] private DiceModelDisplayView _displayView;
        [SerializeField] private ModelRotator _rotator;
        [SerializeField] private TransformFloater _floater;
        [SerializeField] private Image _backdropImage;
        [SerializeField] private Button _backdropCloseButton;
        [SerializeField] private RectTransform _panelTransform;
        [SerializeField] private Image _panelImage;

        [Header("Animation - Fade")]
        [SerializeField] private float _fadeTime = 0.6f;
        [SerializeField] private Ease _fadeEase = Ease.OutQuart;

        [Header("Animation - Scale")]
        [SerializeField] private float _scaleInTime = 0.6f;
        [SerializeField] private Ease _scaleInEase = Ease.OutBack;
        [SerializeField] private float _scaleOutTime = 0.4f;
        [SerializeField] private Ease _scaleOutEase = Ease.OutQuart;

        [Header("Animation - Panel Unfold")]
        [SerializeField] private float _panelUnfoldTime = 0.45f;
        [SerializeField] private Ease _panelUnfoldEase = Ease.OutBack;
        [SerializeField] private float _panelFoldTime = 0.35f;
        [SerializeField] private Ease _panelFoldEase = Ease.InQuart;

        [Header("Animation - Spin Impulse")]
        [SerializeField] private Vector3[] _spinPresets = 
        {
            new(0f, -65f, 110f),
            new(0f, 65f, -183.9f),
            new(135f, -140f, 220f),
            new(25f, -90f, 120f),
            new(-15f, 60f, -80f),
            new(80f, 30f, -90f)
        };
        [SerializeField] private float _spinTime = 0.7f;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        private Quaternion _defaultRotation;
        private Vector3 _defaultScale;
        private float _backdropTargetAlpha;

        private IReadOnlyDiceData _currentData;
        private bool _isOpen;
        private bool _isInitialized;
        private int _lastSpinIndex = -1;

        private Sequence _closeSequence;
        private Tween _diceScaleTween;
        private Tween _spinTween;
        private Tween _backdropFadeTween;
        private Tween _panelUnfoldTween;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Close();
        }

        private void OnDestroy()
        {
            KillAllTweens();

            if (_backdropCloseButton != null)
                _backdropCloseButton.onClick.RemoveListener(Close);

            if (_rotator != null)
                _rotator.InteractionStarted -= CancelSpinAnimation;
        }

        public void Open(IReadOnlyDiceData data)
        {
            EnsureInitialized();

            if (_isOpen && _currentData == data)
                return;

            var isSwitchingDice = _isOpen && _currentData != data;
            _currentData = data;
            _isOpen = true;

            SetRaycastsActive(true);

            _closeSequence?.Kill();
            KillDiceTweens();

            gameObject.SetActive(true);
            _rotator.enabled = true;
            _rotator.ResetVelocity();
            _displayView.Bind(data);

            PrepareDiceTransform();

            _floater.Play();

            if (!isSwitchingDice)
            {
                PlayBackdropFadeIn();
                _panelUnfoldTween?.Kill();
                _panelUnfoldTween = _panelTransform.DoUnfoldX(time: _panelUnfoldTime, ease: _panelUnfoldEase);
            }

            _diceScaleTween = _displayView.transform
                .DoScaleIn(_scaleInTime, _scaleInEase)
                .SetLink(gameObject);

            PlaySpinImpulse();
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            _currentData = null;

            SetRaycastsActive(false);

            KillAllTweens();
            _rotator.enabled = false;
            _floater.StopInstant();
            _rotator.ResetVelocity();

            _backdropFadeTween = _backdropImage
                .DOFade(0f, _fadeTime)
                .SetEase(_fadeEase)
                .SetLink(gameObject);

            _panelUnfoldTween = _panelTransform
                .DoFoldX(_panelFoldTime, _panelFoldEase)
                .SetLink(gameObject);

            _closeSequence = DOTween.Sequence()
                .Join(_displayView.transform.DoScaleOut(_scaleOutTime, _scaleOutEase))
                .Join(_panelUnfoldTween)
                .Join(_backdropFadeTween)
                .OnComplete(ResetAndDeactivate)
                .SetLink(gameObject);
        }

        private void SetRaycastsActive(bool isActive)
        {
            _backdropImage.raycastTarget = isActive;
            _backdropCloseButton.interactable = isActive;

            if (_panelImage != null)
                _panelImage.raycastTarget = isActive;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized) return;

            _defaultRotation = _displayView.transform.localRotation;
            _defaultScale = _displayView.transform.localScale;

            _backdropTargetAlpha = _backdropImage.color.a;

            var transparentColor = _backdropImage.color;
            transparentColor.a = 0f;
            _backdropImage.color = transparentColor;

            _backdropCloseButton.onClick.AddListener(Close);
            _rotator.InteractionStarted += CancelSpinAnimation;

            _isInitialized = true;
        }

        private void CancelSpinAnimation() => _spinTween?.Kill();

        private void PlayBackdropFadeIn()
        {
            _backdropFadeTween?.Kill();
            _backdropFadeTween = _backdropImage
                .DOFade(_backdropTargetAlpha, _fadeTime)
                .SetEase(_fadeEase)
                .SetLink(gameObject);
        }

        private void PrepareDiceTransform()
        {
            _displayView.transform.localScale = _defaultScale;

            var randomIndex = Random.Range(0, _spinPresets.Length - 1);
            if (randomIndex >= _lastSpinIndex && _lastSpinIndex != -1)
                randomIndex++;

            _lastSpinIndex = randomIndex;
            var randomAxis = _spinPresets[randomIndex];
            var targetEuler = _defaultRotation.eulerAngles;
            _displayView.transform.localEulerAngles = targetEuler + randomAxis;
        }

        private void PlaySpinImpulse()
        {
            _spinTween = _displayView.transform
                .DOLocalRotate(_defaultRotation.eulerAngles, _spinTime, RotateMode.FastBeyond360)
                .SetEase(_spinEase)
                .SetLink(gameObject);
        }

        private void ResetAndDeactivate()
        {
            _rotator.ResetVelocity();
            _floater.StopInstant();
            _displayView.transform.localScale = _defaultScale;
            _displayView.transform.localRotation = _defaultRotation;
            _panelTransform.localScale = Vector3.one;
            gameObject.SetActive(false);
        }

        private void KillDiceTweens()
        {
            _diceScaleTween?.Kill();
            _spinTween?.Kill();
        }

        private void KillAllTweens()
        {
            KillDiceTweens();
            _closeSequence?.Kill();
            _backdropFadeTween?.Kill();
            _panelUnfoldTween?.Kill();
        }
    }
}