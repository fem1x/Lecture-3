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
    [SerializeField] private int _value = 0;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
    private void Awake() => _rb = GetComponent<Rigidbody>();
    
    public void Roll(Vector3 force, Vector3 torque)
    {
        _rb.isKinematic = false;
        _rb.AddForce(force, ForceMode.Impulse);
        _rb.AddTorque(torque, ForceMode.Impulse);
    }

    public int GetValue()
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
        _value = bestValue;
        Debug.Log(_value);
        return bestValue;
    }
}
