using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dice : MonoBehaviour
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly (int Value, Vector3 Direction)[] _faces = 
    {
        (1, Vector3.up),
        (6, Vector3.down),
        (5, Vector3.right),
        (2, Vector3.left),
        (3, Vector3.forward),
        (4, Vector3.back)
    };
    
    private Rigidbody _rb;
    private Coroutine _stopCoroutine;
    
    public int Value { get; private set; } = -1;
    public bool IsStopped => _rb.isKinematic || (_rb.linearVelocity.sqrMagnitude < 0.01f && _rb.angularVelocity.sqrMagnitude < 0.01f);
    public event Action<Dice, int> OnStopped;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
    private void Awake() => _rb = GetComponent<Rigidbody>();
    
    public void Roll(Vector3 force, Vector3 torque)
    {
        _rb.isKinematic = false;
        _rb.AddForce(force, ForceMode.Impulse);
        _rb.AddTorque(torque, ForceMode.Impulse);
        
        if (_stopCoroutine != null)
            StopCoroutine(_stopCoroutine);
        _stopCoroutine = StartCoroutine(Co_WaitUntilStopped());
    }
    
    private IEnumerator Co_WaitUntilStopped()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => IsStopped);

        UpdateValue();
        OnStopped?.Invoke(this, Value);
        _stopCoroutine = null;
    }

    public void UpdateValue()
    {
        int bestValue = 1;
        float maxDot = -1f;

        for (int i = 0; i < _faces.Length; i++)
        {
            Vector3 worldDirection = transform.TransformDirection(_faces[i].Direction);
            float dot = Vector3.Dot(worldDirection, Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestValue = _faces[i].Value;
            }
        }
        Value = bestValue;
    }
}
