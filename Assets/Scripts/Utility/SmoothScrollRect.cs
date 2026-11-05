using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class SmoothScrollRect : MonoBehaviour
{
    [SerializeField] private float _smoothTime = 0.08f;
    [SerializeField] private float _scrollMultiplier = 0.001f;
    
    private ScrollRect _scrollRect;
    private float _targetPosition;
    private float _velocity;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
        _targetPosition = _scrollRect.verticalNormalizedPosition;
    }

    private void OnEnable()
    {
        _targetPosition = _scrollRect.verticalNormalizedPosition;
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        float scrollDelta = Mouse.current.scroll.ReadValue().y;
        
        if (Mathf.Abs(scrollDelta) > 0.01f)
        {
            _targetPosition += scrollDelta * _scrollMultiplier * (_scrollRect.scrollSensitivity * 0.1f);
            _targetPosition = Mathf.Clamp01(_targetPosition);
        }

        _scrollRect.verticalNormalizedPosition = Mathf.SmoothDamp(
            _scrollRect.verticalNormalizedPosition,
            _targetPosition,
            ref _velocity,
            _smoothTime
        );
    }
}