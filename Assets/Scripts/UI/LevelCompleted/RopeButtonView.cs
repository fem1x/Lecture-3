using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Workshop
{
    public class RopeButtonView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("References")]
        [SerializeField] private RectTransform _segmentsRoot;
        [SerializeField] private RectTransform _anchor;
        [SerializeField] private RectTransform _buttonRect;
        [SerializeField] private Button _button;
        [SerializeField] private Image _segmentPrefab;

        [Header("Rope Layout")]
        [SerializeField, Min(2)] private int _nodeCount = 16;
        [SerializeField] private float _segmentLength = 12f;
        [SerializeField] private float _segmentWidth = 8f;

        [Header("Physics & Elasticity")]
        [SerializeField] private float _gravity = 1600f;
        [SerializeField, Range(0.8f, 0.999f)] private float _damping = 0.965f;
        [SerializeField, Min(1)] private int _solverIterations = 3;
        [SerializeField, Range(0.05f, 1f)] private float _elasticity = 0.35f;
        [SerializeField] private float _restSpringPull = 12f;
        [SerializeField] private float _inertiaMultiplier = 0.7f;

        [Header("Pull Impulse")]
        [SerializeField] private float _pullDownImpulse = 1400f;
        [SerializeField] private float _pullSideImpulse = 320f;

        [Header("Retraction")]
        [SerializeField, Min(0.01f)] private float _retractDuration = 0.8f;
        [SerializeField, Min(0f)] private float _retractDistance = 1200f;
        [SerializeField] private Ease _retractEase = Ease.InCubic;
        [SerializeField, Min(0f), Tooltip("Seconds to wait after retraction before deactivating the rope.")]
        private float _retractHideDelay = 0.25f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnPulled;

        private readonly List<Vector2> _currentPositions = new();
        private readonly List<Vector2> _previousPositions = new();
        private readonly List<RectTransform> _segments = new();

        private Vector2 _lastRootPos;
        private Vector3 _lastRootWorldPos;
        private bool _isBuilt;
        private int _pullDirSign = 1;
        private Vector2 _anchorStartPosition;
        private bool _isRetracting;
        private CancellationTokenSource _retractCts;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _anchorStartPosition = _anchor.anchoredPosition;
            BuildSegments();
            CollapseToAnchor();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClick);
            _lastRootPos = _segmentsRoot.anchoredPosition;
            _lastRootWorldPos = _segmentsRoot.position;
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClick);
            CancelRetraction();
        }

        // Update the chain after DOTween moves its anchor in Update.
        private void LateUpdate()
        {
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            if (dt <= 0f) return;

            var currentRootPos = _segmentsRoot.anchoredPosition;
            var rootDelta = currentRootPos - _lastRootPos;
            _lastRootPos = currentRootPos;

            if (_isRetracting)
            {
                var localDelta = (Vector2)_segmentsRoot.InverseTransformVector(_segmentsRoot.position - _lastRootWorldPos);
                for (var i = 0; i < _nodeCount; i++)
                {
                    _currentPositions[i] -= localDelta;
                    _previousPositions[i] -= localDelta;
                }
            }
            _lastRootWorldPos = _segmentsRoot.position;

            StepVerlet(dt, rootDelta);
            SyncVisuals();
        }

        private bool CanPreviewRope => Application.isPlaying && isActiveAndEnabled && !_isRetracting;

        [InspectorButton]
        [ContextMenu("Redeploy Rope")]
        public void RedeployRope()
        {
            if (CanPreviewRope) CollapseToAnchor();
        }

        [InspectorButton]
        [ContextMenu("Test Rope Retraction")]
        public void TestRopeRetraction()
        {
            if (!CanPreviewRope) return;
            RetractAsync(destroyCancellationToken, hideAfter: true).Forget();
        }

        public void CollapseToAnchor()
        {
            CancelRetraction();
            BuildSegments();

            _isRetracting = false;
            _anchor.anchoredPosition = _anchorStartPosition;
            _button.interactable = true;

            var anchorLocal = GetAnchorInSegmentsRootSpace();
            _lastRootPos = _segmentsRoot.anchoredPosition;
            _lastRootWorldPos = _segmentsRoot.position;

            for (var i = 0; i < _nodeCount; i++)
            {
                var jitter = i == 0 ? 0f : Mathf.Sin(i * 1.7f) * 1.5f;
                var pos = anchorLocal + new Vector2(jitter, -i * 0.5f);
                _currentPositions[i] = pos;
                _previousPositions[i] = pos;
            }

            SyncVisuals();
        }

        public async UniTask RetractAsync(CancellationToken ct, bool hideAfter = true)
        {
            if (!gameObject.activeInHierarchy) return;

            CancelRetraction();
            var retraction = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            _retractCts = retraction;
            ct = retraction.Token;
            _isRetracting = true;
            _button.interactable = false;
            var start = _anchor.anchoredPosition;
            try
            {
                var tween = _anchor.DOAnchorPos(start + Vector2.up * _retractDistance, _retractDuration)
                    .SetEase(_retractEase)
                    .SetUpdate(UpdateType.Normal, isIndependentUpdate: true);
                await tween.ToUniTask(cancellationToken: ct);
                // Let LateUpdate apply the last anchor movement before hiding the rope.
                await UniTask.NextFrame(ct);
                if (!hideAfter) return;

                if (_retractHideDelay > 0f)
                    await UniTask.Delay(TimeSpan.FromSeconds(_retractHideDelay), ignoreTimeScale: true, cancellationToken: ct);

                ct.ThrowIfCancellationRequested();
                gameObject.SetActive(false);
            }
            finally
            {
                if (_retractCts == retraction)
                {
                    _retractCts = null;
                    _isRetracting = false;
                }
                retraction.Dispose();
            }
        }

        private void CancelRetraction()
        {
            var retraction = _retractCts;
            _retractCts = null;
            _isRetracting = false;
            retraction?.Cancel();
        }

        private void BuildSegments()
        {
            if (_isBuilt) return;

            _currentPositions.Clear();
            _previousPositions.Clear();
            _segments.Clear();

            for (var i = 0; i < _nodeCount; i++)
            {
                _currentPositions.Add(Vector2.zero);
                _previousPositions.Add(Vector2.zero);
            }

            var segmentCount = _nodeCount - 1;
            for (var i = 0; i < segmentCount; i++)
            {
                var seg = Instantiate(_segmentPrefab, _segmentsRoot);
                seg.gameObject.SetActive(true);
                seg.rectTransform.pivot = new(0.5f, 0.5f);
                _segments.Add(seg.rectTransform);
            }

            _isBuilt = true;
        }

        private void StepVerlet(float dt, Vector2 rootDelta)
        {
            var anchorLocal = GetAnchorInSegmentsRootSpace();
            var dtSq = dt * dt;

            for (var i = 0; i < _nodeCount; i++)
            {
                var curr = _currentPositions[i];
                var prev = _previousPositions[i] - rootDelta * _inertiaMultiplier;

                var velocity = (curr - prev) * _damping;
                var springToRest = i > 0
                    ? new Vector2((anchorLocal.x - curr.x) * _restSpringPull, 0f)
                    : Vector2.zero;

                var accel = new Vector2(0f, -_gravity) + springToRest;
                var next = curr + velocity + accel * dtSq;

                _previousPositions[i] = curr;
                _currentPositions[i] = next;
            }

            for (var iter = 0; iter < _solverIterations; iter++)
            {
                _currentPositions[0] = anchorLocal;
                _previousPositions[0] = anchorLocal;

                for (var i = 0; i < _nodeCount - 1; i++)
                {
                    var a = _currentPositions[i];
                    var b = _currentPositions[i + 1];
                    var delta = b - a;
                    var dist = delta.magnitude;
                    if (dist <= 0.0001f) continue;

                    var diff = ((dist - _segmentLength) / dist) * _elasticity;

                    if (i == 0)
                    {
                        _currentPositions[i + 1] = b - delta * diff;
                    }
                    else
                    {
                        _currentPositions[i] = a + delta * (diff * 0.5f);
                        _currentPositions[i + 1] = b - delta * (diff * 0.5f);
                    }
                }
            }
        }

        private void SyncVisuals()
        {
            for (var i = 0; i < _segments.Count; i++)
            {
                var a = _currentPositions[i];
                var b = _currentPositions[i + 1];
                var mid = (a + b) * 0.5f;
                var diff = b - a;
                var len = diff.magnitude;
                var angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90f;

                var segRect = _segments[i];
                segRect.anchoredPosition = mid;
                segRect.localRotation = Quaternion.Euler(0f, 0f, angle);
                segRect.sizeDelta = new(_segmentWidth, Mathf.Max(len, 0.01f));
            }

            var tipLocal = _currentPositions[_nodeCount - 1];
            _buttonRect.anchoredPosition = tipLocal;
        }

        private Vector2 GetAnchorInSegmentsRootSpace()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _segmentsRoot,
                RectTransformUtility.WorldToScreenPoint(null, _anchor.position),
                null,
                out var localPoint);

            return localPoint;
        }

        private void HandleButtonClick()
        {
            if (_isRetracting) return;
            var lastIdx = _nodeCount - 1;
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.016f);
            if (dt <= 0f) dt = 0.016f;

            _pullDirSign = -_pullDirSign;
            var impulse = new Vector2(_pullDirSign * _pullSideImpulse, -_pullDownImpulse) * dt;
            _previousPositions[lastIdx] -= impulse;

            OnPulled?.Invoke();
        }
    }
}
