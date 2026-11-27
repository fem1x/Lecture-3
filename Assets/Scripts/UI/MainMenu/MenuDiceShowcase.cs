using _Scripts.Dices;
using _Scripts.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.MainMenu
{
    public sealed class MenuDiceShowcase : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, ICancelHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private RawImage _output;
        [SerializeField] private Transform _diceVisual;
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

        [Header("Lighting")]
        [SerializeField, Min(0f)] private float _keyIntensity = 42f;
        [SerializeField, Min(0f)] private float _fillIntensity = 16f;
        [SerializeField, Min(0f)] private float _rimIntensity = 28f;

        private const uint StudioRenderingLayer = 1u << 7;

        private static readonly Vector3[] Positions =
        {
            new(-0.6f, 0.02f, 0f), new(1.23f, 0.95f, 0.2f), new(0.9f, -1.42f, 0.45f)
        };

        private static readonly Vector3[] Angles =
        {
            new(24f, -34f, 14f), new(-32f, 26f, -16f), new(20f, 42f, -20f)
        };

        private static readonly float[] Sizes = { 1.55f, 1.02f, 0.83f };
        private static readonly int[] Durability = { 1, 2, 3 };
        private static readonly Bounds DieBounds = new(Vector3.zero, Vector3.one * 1.04f);

        private readonly Transform[] _dice = new Transform[3];
        private readonly ModelRotator[] _rotators = new ModelRotator[3];
        private readonly float[] _appearance = new float[3];
        private readonly float[] _touch = new float[3];
        private readonly float[] _rollProgress = new float[3];
        private GameObject _studio;
        private Camera _camera;
        private Light _key;
        private RenderTexture _texture;
        private Sequence _roll;
        private Sequence _entrance;
        private ISfxPlayer _sfx;
        private Vector2 _parallax;
        private float _time;
        private float _focus;
        private bool _playHovered;
        private bool _interactionEnabled;
        private bool _pointerOnStage;
        private int _heldDie = -1;
        private int _pointerId;
        private float _lastDragTime;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(ISfxPlayer sfx) => _sfx = sfx;

        private void OnEnable()
        {
            if (_studio == null) CreateStudio();
            _studio.SetActive(true);
            _output.enabled = true;
        }

        private void OnDisable()
        {
            _roll?.Kill();
            _entrance?.Kill();
            ReleaseDie();
            _pointerOnStage = false;
            _output.enabled = false;
            if (_studio != null) _studio.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_studio != null) Destroy(_studio);
            if (_texture == null) return;
            _texture.Release();
            Destroy(_texture);
        }

        private void Update()
        {
            var delta = Time.unscaledDeltaTime;
            _time += delta;
            var mouse = Mouse.current;
            var position = mouse != null ? mouse.position.ReadValue() : new Vector2(Screen.width, Screen.height) * 0.5f;
            var pointer = new Vector2(position.x / Screen.width - 0.5f, position.y / Screen.height - 0.5f);
            pointer = Vector2.ClampMagnitude(pointer, 0.65f);
            var smoothing = 1f - Mathf.Exp(-6f * delta);
            var targetParallax = _interactionEnabled ? pointer * _cursorParallaxStrength : Vector2.zero;
            if (_heldDie >= 0) targetParallax = _parallax;
            _parallax = _cursorParallaxStrength > 0f
                ? Vector2.Lerp(_parallax, targetParallax, smoothing)
                : Vector2.zero;
            _focus = Mathf.Lerp(_focus, _playHovered ? 1f : 0f, smoothing);
            _camera.transform.localPosition = new Vector3(_parallax.x * 0.58f, _parallax.y * 0.46f, -9f);
            _key.intensity = _keyIntensity * (1f + _focus * 0.23f);
            var hovered = _pointerOnStage && _interactionEnabled && mouse != null
                ? PickDie(mouse.position.ReadValue(), null)
                : -1;
            UpdateDice(hovered, delta);
        }

        private void UpdateDice(int hovered, float delta)
        {
            for (var i = 0; i < _dice.Length; i++)
            {
                var appearance = _appearance[i];
                var feedback = i == _heldDie ? -0.04f : i == hovered ? 0.045f : 0f;
                _touch[i] = Mathf.Lerp(_touch[i], feedback, 1f - Mathf.Exp(-24f * delta));
                var phase = _time * (1.05f + 0.14f * i) + i * 2.1f;
                var progress = _rollProgress[i];
                var bounce = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * _rollLift * (Sizes[i] / Sizes[0]);
                var spin = _rollDegrees * progress;
                if (i == 1) spin = new Vector3(spin.x, -spin.y, -spin.z);
                if (i == 2) spin = -spin;
                var enter = 1f - Mathf.Min(1f, appearance);
                _dice[i].localScale = Vector3.one * (Sizes[i] * appearance * (1f + _touch[i]));
                _dice[i].localPosition = Positions[i] + new Vector3(0.09f * Mathf.Cos(phase),
                    0.13f * Mathf.Sin(phase) + bounce - enter * 0.44f, 0f);
                _dice[i].localRotation = Quaternion.Euler(Angles[i] + spin +
                    new Vector3(10f * Mathf.Sin(phase * 0.7f) - _focus * 8f,
                        17f * Mathf.Sin(phase * 0.61f) + enter * 24f + _focus * (i == 1 ? -12f : 12f),
                        5f * Mathf.Cos(phase)));
            }
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        internal void SetPlayHovered(bool hovered) => _playHovered = hovered;

        internal void SetInteractionEnabled(bool enabled)
        {
            _interactionEnabled = enabled;
            _output.raycastTarget = enabled;
            if (!enabled) ReleaseDie();
        }

        internal void Show()
        {
            _roll?.Kill();
            _entrance?.Kill();
            ReleaseDie();
            _time = 0f;
            _focus = 0f;
            _playHovered = false;
            _parallax = Vector2.zero;
            _entrance = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var i = 0; i < _dice.Length; i++)
            {
                var index = i;
                _appearance[i] = 0f;
                _rollProgress[i] = 0f;
                _touch[i] = 0f;
                _rotators[i].ResetVelocity();
                _rotators[i].transform.localRotation = Quaternion.identity;
                var start = _entranceDelay + i * _entranceStagger;
                _entrance.InsertCallback(start, () => _sfx.Play("DiceAppear"));
                _entrance.Insert(start,
                    DOTween.To(() => _appearance[index], value => _appearance[index] = value, 1f, _entranceDuration)
                        .SetEase(_entranceEase).SetUpdate(true).SetLink(gameObject));
            }
            UpdateDice(-1, 0f);
        }

        internal Tween PlayRoll()
        {
            _roll?.Kill();
            _roll = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            for (var i = 0; i < _rollProgress.Length; i++)
            {
                var index = i;
                _rollProgress[i] = 0f;
                _roll.Insert(_rollDelay + i * _rollStagger,
                    DOTween.To(() => _rollProgress[index], value => _rollProgress[index] = value, 1f, _rollDuration)
                        .SetEase(_rollEase).SetUpdate(true).SetLink(gameObject));
            }
            return _roll;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactionEnabled || _heldDie >= 0 || eventData.button != PointerEventData.InputButton.Left)
                return;

            _heldDie = PickDie(eventData.position, eventData.pressEventCamera);
            if (_heldDie < 0) return;
            _pointerId = eventData.pointerId;
            _lastDragTime = Time.unscaledTime;
            _rotators[_heldDie].BeginDrag();
            _sfx.PlayUI("ButtonClick", _output.rectTransform);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_heldDie < 0 || eventData.pointerId != _pointerId) return;
            var referenceDelta = eventData.delta * (1080f / Mathf.Max(1, Screen.height));
            _rotators[_heldDie].Drag(referenceDelta);
            if (referenceDelta.sqrMagnitude > 0.01f) _lastDragTime = Time.unscaledTime;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.pointerId == _pointerId)
                ReleaseDie(keepMomentum: true);
        }

        public void OnEndDrag(PointerEventData eventData) => OnPointerUp(eventData);
        public void OnPointerEnter(PointerEventData eventData) => _pointerOnStage = true;
        public void OnPointerExit(PointerEventData eventData) => _pointerOnStage = false;
        public void OnCancel(BaseEventData eventData) => ReleaseDie();
        private void OnApplicationFocus(bool focused)
        {
            if (!focused) ReleaseDie();
        }

        private void ReleaseDie(bool keepMomentum = false)
        {
            if (_heldDie < 0) return;
            var rotator = _rotators[_heldDie];
            if (!keepMomentum || Time.unscaledTime - _lastDragTime > 0.075f)
                rotator.ResetVelocity();
            rotator.EndDrag();
            _heldDie = -1;
        }

        private int PickDie(Vector2 screenPoint, Camera eventCamera)
        {
            var rect = _output.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out var local) ||
                !rect.rect.Contains(local)) return -1;

            var uv = Rect.PointToNormalized(rect.rect, local);
            var ray = _camera.ViewportPointToRay(uv);
            var picked = -1;
            var nearest = float.PositiveInfinity;
            for (var i = 0; i < _rotators.Length; i++)
            {
                if (_appearance[i] < 0.7f) continue;
                var pose = _rotators[i].transform;
                var localRay = new Ray(pose.InverseTransformPoint(ray.origin), pose.InverseTransformDirection(ray.direction));
                if (!DieBounds.IntersectRay(localRay, out var distance)) continue;
                // Convert the local hit distance back to world units before comparing dice.
                distance *= pose.lossyScale.x;
                if (distance >= nearest) continue;
                picked = i;
                nearest = distance;
            }
            return picked;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void CreateStudio()
        {
            // Separating the camera by distance also keeps other preview cameras out of the studio.
            var layer = LayerMask.NameToLayer("DicePreview");
            _studio = new GameObject("Main Menu Dice Studio");
            _studio.transform.position = new Vector3(0f, -500f, 0f);
            _studio.SetActive(false);

            _texture = new RenderTexture(900, 800, 24, RenderTextureFormat.ARGB32)
            {
                name = "Main Menu Dice", antiAliasing = 4
            };
            _output.texture = _texture;
            _camera = AddObject("Camera", layer, _studio.transform).AddComponent<Camera>();
            _camera.transform.localPosition = new Vector3(0f, 0f, -9f);
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.backgroundColor = Color.clear;
            _camera.orthographic = true;
            _camera.orthographicSize = 2.65f;
            _camera.nearClipPlane = 0.1f;
            _camera.farClipPlane = 18f;
            _camera.allowHDR = false;
            _camera.useOcclusionCulling = false;
            _camera.cullingMask = 1 << layer;
            _camera.targetTexture = _texture;

            _key = AddLight("Key", layer, new Vector3(-2.8f, 3.3f, -3.6f), new Color(1f, 0.93f, 0.88f), _keyIntensity);
            AddLight("Rim", layer, new Vector3(3.2f, 1.9f, 1.5f), new Color(0.83f, 0.63f, 1f), _rimIntensity);
            AddLight("Fill", layer, new Vector3(2.3f, -2.1f, -3.2f), new Color(0.57f, 0.65f, 1f), _fillIntensity);

            for (var i = 0; i < _dice.Length; i++)
            {
                var model = AddObject($"Die {i + 1}", layer, _studio.transform).transform;
                model.localPosition = Positions[i];
                model.localRotation = Quaternion.Euler(Angles[i]);
                model.localScale = Vector3.one * Sizes[i];
                var rotator = AddObject("Rotation", layer, model).AddComponent<ModelRotator>();
                rotator.ConfigurePointerDrag(_camera, _dragSensitivity, _dragDamping);
                foreach (Transform part in _diceVisual)
                {
                    if (part.TryGetComponent<DiceFaceView>(out var face))
                    {
                        var clone = Instantiate(face, rotator.transform, false);
                        clone.SetCracksLevel(Durability[i], 3);
                        SetLayer(clone.transform, layer);
                    }
                    else
                        CopyVisual(part, rotator.transform, layer);
                }
                _dice[i] = model;
                _rotators[i] = rotator;
            }
        }

        private Light AddLight(string name, int layer, Vector3 position, Color color, float intensity)
        {
            var light = AddObject(name, layer, _studio.transform).AddComponent<Light>();
            light.transform.localPosition = position;
            light.type = LightType.Point;
            light.cullingMask = 1 << layer;
            light.GetUniversalAdditionalLightData().renderingLayers = (RenderingLayerMask)StudioRenderingLayer;
            light.color = color;
            light.range = 12f;
            light.intensity = intensity;
            return light;
        }

        private static GameObject AddObject(string name, int layer, Transform parent)
        {
            var child = new GameObject(name) { layer = layer };
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void SetLayer(Transform part, int layer)
        {
            part.gameObject.layer = layer;
            if (part.TryGetComponent<Renderer>(out var renderer))
                renderer.renderingLayerMask = StudioRenderingLayer;
            foreach (Transform child in part)
                SetLayer(child, layer);
        }

        private static void CopyVisual(Transform part, Transform parent, int layer)
        {
            if (!part.gameObject.activeSelf) return;
            var copy = AddObject(part.name, layer, parent);
            copy.transform.SetLocalPositionAndRotation(part.localPosition, part.localRotation);
            copy.transform.localScale = part.localScale;

            if (part.TryGetComponent<MeshRenderer>(out var mesh) && mesh.enabled)
            {
                copy.AddComponent<MeshFilter>().sharedMesh = part.GetComponent<MeshFilter>().sharedMesh;
                var renderer = copy.AddComponent<MeshRenderer>();
                renderer.renderingLayerMask = StudioRenderingLayer;
                renderer.sharedMaterials = mesh.sharedMaterials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            }

            if (part.TryGetComponent<SpriteRenderer>(out var sprite) && sprite.enabled)
            {
                var renderer = copy.AddComponent<SpriteRenderer>();
                renderer.renderingLayerMask = StudioRenderingLayer;
                renderer.sprite = sprite.sprite;
                renderer.sharedMaterial = sprite.sharedMaterial;
                renderer.color = sprite.color;
                renderer.drawMode = sprite.drawMode;
                renderer.size = sprite.size;
                renderer.flipX = sprite.flipX;
                renderer.flipY = sprite.flipY;
                renderer.sortingOrder = sprite.sortingOrder;
            }

            foreach (Transform child in part)
                CopyVisual(child, copy.transform, layer);
        }
    }
}
