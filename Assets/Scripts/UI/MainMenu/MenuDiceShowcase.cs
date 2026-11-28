using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.MainMenu
{
    public sealed class MenuDiceShowcase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IPointerMoveHandler,
        IPointerExitHandler, ICancelHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [System.Serializable]
        private sealed class Die
        {
            public Transform Entrance;
            public Transform Idle;
            public Transform Focus;
            public Transform Shake;
            public Transform Roll;
            public Transform Touch;
            public ModelRotator Rotator;
        }

        [SerializeField] private RawImage _output;
        [SerializeField] private GameObject _studio;
        [SerializeField] private Camera _camera;
        [SerializeField] private Light _key;
        [SerializeField] private Die[] _dice;
        [SerializeField, Range(0f, 2f)] private float _cursorParallaxStrength = 1f;
        [SerializeField, Min(0.01f)] private float _dragSensitivity = 0.42f;
        [SerializeField, Min(0.01f)] private float _dragDamping = 9f;

        [Header("Dice Entrance")]
        [SerializeField, Min(0f)] private float _entranceDelay = 0.16f;
        [SerializeField, Min(0.01f)] private float _entranceDuration = 0.45f;
        [SerializeField, Min(0f)] private float _entranceStagger = 0.09f;
        [SerializeField] private Ease _entranceEase = Ease.OutBack;

        [Header("Play Roll")]
        [SerializeField, Min(0f)] private float _rollDelay;
        [SerializeField, Min(0.01f)] private float _rollDuration = 0.53f;
        [SerializeField, Min(0f)] private float _rollStagger = 0.07f;
        [SerializeField] private Ease _rollEase = Ease.InOutSine;
        [SerializeField] private Vector3 _rollDegrees = new(360f, 260f, 45f);
        [SerializeField, Min(0f)] private float _rollLift = 0.55f;

        [Header("Exit Hover Shake")]
        [SerializeField, Min(0f)] private float _shakeFrequency = 26f;
        [SerializeField, Min(0f)] private float _shakeStrength = 1f;

        [Header("Lighting")]
        [SerializeField, Min(0f)] private float _keyIntensity = 42f;

        private static readonly Bounds DieBounds = new(Vector3.zero, Vector3.one * 1.04f);
        private Sequence _roll;
        private Sequence _entrance;
        private readonly List<Tween> _idle = new();
        private Sequence _focusTween;
        private readonly List<Tween> _shakeTweens = new();
        private readonly Tween[] _touchTweens = new Tween[3];
        private ISfxPlayer _sfx;
        private Vector2 _targetParallax;
        private bool _playHovered;
        private bool _exitHovered;
        private bool _interactionEnabled;
        private int _hoveredDie = -1;
        private int _heldDie = -1;
        private int _pointerId;
        private float _lastDragTime;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(ISfxPlayer sfx) => _sfx = sfx;

        private void OnEnable()
        {
            _studio.SetActive(true);
            _output.enabled = true;
            foreach (var die in _dice)
                die.Rotator.ConfigurePointerDrag(_camera, _dragSensitivity, _dragDamping);
        }

        private void OnDisable()
        {
            _roll?.Kill();
            _entrance?.Kill();
            KillTweens(_idle);
            _focusTween?.Kill();
            KillTweens(_shakeTweens);
            ReleaseDie();
            foreach (var tween in _touchTweens) tween?.Kill();
            _hoveredDie = -1;
            _output.enabled = false;
            _studio.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_cursorParallaxStrength <= 0f)
            {
                _camera.transform.localPosition = new Vector3(0f, 0f, -9f);
                return;
            }
            if (!_interactionEnabled || _heldDie >= 0) return;
            var mouse = Mouse.current;
            if (mouse == null) return;
            var position = mouse.position.ReadValue();
            var pointer = new Vector2(position.x / Screen.width - 0.5f, position.y / Screen.height - 0.5f);
            _targetParallax = Vector2.ClampMagnitude(pointer, 0.65f) * _cursorParallaxStrength;
            var target = new Vector3(_targetParallax.x * 0.58f, _targetParallax.y * 0.46f, -9f);
            _camera.transform.localPosition = Vector3.Lerp(_camera.transform.localPosition, target,
                1f - Mathf.Exp(-6f * Time.unscaledDeltaTime));
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        internal void SetButtonHover(bool playHovered, bool exitHovered)
        {
            if (_playHovered != playHovered)
            {
                _playHovered = playHovered;
                _focusTween?.Kill();
                _focusTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
                for (var i = 0; i < _dice.Length; i++)
                {
                    var focus = _dice[i].Focus;
                    var rotation = playHovered ? new Vector3(-17f, i == 1 ? -24f : 24f, 0f) : Vector3.zero;
                    _focusTween.Join(focus.DOScale(playHovered ? 1.1f : 1f, 0.2f).SetUpdate(true).SetLink(gameObject));
                    _focusTween.Join(focus.DOLocalMoveY(playHovered ? 0.12f : 0f, 0.2f).SetUpdate(true).SetLink(gameObject));
                    _focusTween.Join(focus.DOLocalRotate(rotation, 0.2f).SetUpdate(true).SetLink(gameObject));
                }
                _focusTween.Join(_key.DOIntensity(_keyIntensity * (playHovered ? 1.35f : 1f), 0.2f)
                    .SetUpdate(true).SetLink(gameObject));
            }

            if (_exitHovered == exitHovered) return;
            _exitHovered = exitHovered;
            KillTweens(_shakeTweens);
            foreach (var die in _dice)
            {
                die.Shake.localPosition = Vector3.zero;
                die.Shake.localRotation = Quaternion.identity;
                if (exitHovered && _shakeStrength > 0f && _shakeFrequency > 0f)
                {
                    _shakeTweens.Add(die.Shake.DOShakePosition(0.5f, 0.035f * _shakeStrength,
                        Mathf.Max(1, Mathf.RoundToInt(_shakeFrequency * 0.5f)), 90f, false, false)
                        .SetLoops(-1, LoopType.Restart).SetUpdate(true).SetLink(gameObject));
                }
            }
        }

        internal void SetInteractionEnabled(bool enabled)
        {
            _interactionEnabled = enabled;
            _output.raycastTarget = enabled;
            if (enabled) return;
            SetButtonHover(false, false);
            SetHoveredDie(-1);
            ReleaseDie();
        }

        internal Sequence Show()
        {
            _studio.SetActive(true);
            _output.enabled = true;
            _roll?.Kill();
            _entrance?.Kill();
            KillTweens(_idle);
            _focusTween?.Kill();
            KillTweens(_shakeTweens);
            ReleaseDie();
            _playHovered = false;
            _exitHovered = false;
            _hoveredDie = -1;
            _camera.transform.localPosition = new Vector3(0f, 0f, -9f);
            _key.intensity = _keyIntensity;
            _entrance = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var i = 0; i < _dice.Length; i++)
            {
                var die = _dice[i];
                _touchTweens[i]?.Kill();
                die.Rotator.ResetVelocity();
                die.Rotator.transform.localRotation = Quaternion.identity;
                die.Entrance.localScale = Vector3.zero;
                die.Entrance.localPosition = Vector3.down * 0.44f;
                die.Entrance.localRotation = Quaternion.Euler(0f, 24f, 0f);
                die.Idle.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                die.Focus.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                die.Focus.localScale = Vector3.one;
                die.Shake.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                die.Roll.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                die.Touch.localScale = Vector3.one;
                var start = _entranceDelay + i * _entranceStagger;
                _entrance.InsertCallback(start, () => _sfx.Play("DiceAppear"));
                _entrance.Insert(start, die.Entrance.DOScale(1f, _entranceDuration).SetEase(_entranceEase).SetUpdate(true).SetLink(gameObject));
                _entrance.Insert(start, die.Entrance.DOLocalMove(Vector3.zero, _entranceDuration).SetEase(_entranceEase).SetUpdate(true).SetLink(gameObject));
                _entrance.Insert(start, die.Entrance.DOLocalRotate(Vector3.zero, _entranceDuration).SetEase(_entranceEase).SetUpdate(true).SetLink(gameObject));
                var duration = 2.4f + i * 0.3f;
                _idle.Add(die.Idle.DOLocalMoveY(0.13f, duration).SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(gameObject));
                _idle.Add(die.Idle.DOLocalRotate(new Vector3(10f, i == 1 ? -17f : 17f, 5f), duration * 1.3f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true).SetLink(gameObject));
            }
            return _entrance;
        }

        internal Tween PlayRoll()
        {
            _roll?.Kill();
            _roll = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var i = 0; i < _dice.Length; i++)
            {
                var roll = _dice[i].Roll;
                roll.localPosition = Vector3.zero;
                roll.localRotation = Quaternion.identity;
                var rotation = i == 1 ? new Vector3(_rollDegrees.x, -_rollDegrees.y, -_rollDegrees.z)
                    : i == 2 ? -_rollDegrees : _rollDegrees;
                var start = _rollDelay + i * _rollStagger;
                _roll.Insert(start, roll.DOLocalRotate(rotation, _rollDuration, RotateMode.FastBeyond360)
                    .SetEase(_rollEase).SetUpdate(true).SetLink(gameObject));
                _roll.Insert(start, roll.DOLocalMoveY(_rollLift, _rollDuration * 0.5f)
                    .SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo).SetUpdate(true).SetLink(gameObject));
            }
            return _roll;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_interactionEnabled && _heldDie < 0)
                SetHoveredDie(PickDie(eventData.position, eventData.enterEventCamera));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactionEnabled || _heldDie >= 0 || eventData.button != PointerEventData.InputButton.Left) return;
            _heldDie = PickDie(eventData.position, eventData.pressEventCamera);
            if (_heldDie < 0) return;
            _pointerId = eventData.pointerId;
            _lastDragTime = Time.unscaledTime;
            _dice[_heldDie].Rotator.BeginDrag();
            AnimateTouch(_heldDie, 0.96f);
            _sfx.PlayUI("ButtonClick", _output.rectTransform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_heldDie < 0 || eventData.pointerId != _pointerId) return;
            var referenceDelta = eventData.delta * (1080f / Mathf.Max(1, Screen.height));
            _dice[_heldDie].Rotator.Drag(referenceDelta);
            if (referenceDelta.sqrMagnitude > 0.01f) _lastDragTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.pointerId == _pointerId)
            {
                ReleaseDie(keepMomentum: true);
                if (_interactionEnabled) SetHoveredDie(PickDie(eventData.position, eventData.pressEventCamera));
            }
        }

        public void OnEndDrag(PointerEventData eventData) => OnPointerUp(eventData);
        public void OnPointerExit(PointerEventData eventData) => SetHoveredDie(-1);
        public void OnCancel(BaseEventData eventData) => ReleaseDie();
        private void OnApplicationFocus(bool focused) { if (!focused) ReleaseDie(); }

        private void SetHoveredDie(int index)
        {
            if (_hoveredDie == index) return;
            if (_hoveredDie >= 0 && _hoveredDie != _heldDie) AnimateTouch(_hoveredDie, 1f);
            _hoveredDie = index;
            if (index >= 0 && index != _heldDie) AnimateTouch(index, 1.045f);
        }

        private void AnimateTouch(int index, float scale)
        {
            _touchTweens[index]?.Kill();
            _touchTweens[index] = _dice[index].Touch.DOScale(scale, 0.12f).SetEase(Ease.OutQuad)
                .SetUpdate(true).SetLink(gameObject);
        }

        private void ReleaseDie(bool keepMomentum = false)
        {
            if (_heldDie < 0) return;
            var index = _heldDie;
            var rotator = _dice[index].Rotator;
            if (!keepMomentum || Time.unscaledTime - _lastDragTime > 0.075f) rotator.ResetVelocity();
            rotator.EndDrag();
            _heldDie = -1;
            AnimateTouch(index, index == _hoveredDie && _interactionEnabled ? 1.045f : 1f);
        }

        private static void KillTweens(List<Tween> tweens)
        {
            foreach (var tween in tweens) tween.Kill();
            tweens.Clear();
        }

        private int PickDie(Vector2 screenPoint, Camera eventCamera)
        {
            var rect = _output.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out var local) ||
                !rect.rect.Contains(local)) return -1;
            var ray = _camera.ViewportPointToRay(Rect.PointToNormalized(rect.rect, local));
            var picked = -1;
            var nearest = float.PositiveInfinity;
            for (var i = 0; i < _dice.Length; i++)
            {
                if (_dice[i].Entrance.localScale.x < 0.7f) continue;
                var pose = _dice[i].Rotator.transform;
                var localRay = new Ray(pose.InverseTransformPoint(ray.origin), pose.InverseTransformDirection(ray.direction));
                if (!DieBounds.IntersectRay(localRay, out var distance)) continue;
                distance *= pose.lossyScale.x;
                if (distance >= nearest) continue;
                picked = i;
                nearest = distance;
            }
            return picked;
        }
    }
}
