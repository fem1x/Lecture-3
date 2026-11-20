using System;
using System.Collections.Generic;
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
        [SerializeField, Min(2)] private int _nodeCount = 10;
        [SerializeField] private float _segmentLength = 16f;
        [SerializeField] private float _segmentWidth = 6f;

        [Header("Physics")]
        [SerializeField] private float _gravity = 1800f;
        [SerializeField, Range(0.8f, 0.999f)] private float _damping = 0.985f;
        [SerializeField, Min(1)] private int _solverIterations = 10;
        [SerializeField] private float _inertiaMultiplier = 0.65f;

        [Header("Pull Impulse")]
        [SerializeField] private float _pullDownImpulse = 950f;
        [SerializeField] private float _pullSideImpulse = 160f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnPulled;

        private readonly List<Vector2> _currentPositions = new();
        private readonly List<Vector2> _previousPositions = new();
        private readonly List<RectTransform> _segments = new();

        private Vector2 _lastRootPos;
        private bool _isBuilt;
        private int _pullDirSign = 1;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            BuildSegments();
            CollapseToAnchor();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClick);
            _lastRootPos = _segmentsRoot.anchoredPosition;
        }

        private void OnDisable() => _button.onClick.RemoveListener(HandleButtonClick);

        private void Update()
        {
            var dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);
            if (dt <= 0f) return;

            var currentRootPos = _segmentsRoot.anchoredPosition;
            var rootDelta = currentRootPos - _lastRootPos;
            _lastRootPos = currentRootPos;

            StepVerlet(dt, rootDelta);
            SyncVisuals();
        }

        public void CollapseToAnchor()
        {
            BuildSegments();

            var anchorLocal = GetAnchorInSegmentsRootSpace();
            _lastRootPos = _segmentsRoot.anchoredPosition;

            for (var i = 0; i < _nodeCount; i++)
            {
                var jitter = i == 0 ? 0f : Mathf.Sin(i * 1.7f) * 1.5f;
                var pos = anchorLocal + new Vector2(jitter, -i * 0.5f);
                _currentPositions[i] = pos;
                _previousPositions[i] = pos;
            }

            SyncVisuals();
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
                var next = curr + velocity + new Vector2(0f, -_gravity * dtSq);

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

                    var diff = (dist - _segmentLength) / dist;

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