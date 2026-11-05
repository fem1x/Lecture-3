using DG.Tweening;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "CameraShakePreset", menuName = "Presets/CameraShakePreset", order = 0)]
    public class CameraShakePreset : ScriptableObject
    {
        [field: Header("Position Shake")]
        [field: SerializeField] public float Duration { get; private set; } = 0.2f;
        [field: SerializeField] public float Strength { get; private set; } = 0.05f;
        [field: SerializeField] public int Vibrato { get; private set; } = 15;
        [field: SerializeField, Range(0f, 180f)] public float Randomness { get; private set; } = 90f;

        [field: Header("Rotation Shake")]
        [field: SerializeField] public bool EnableRotationShake { get; private set; } = false;
        [field: SerializeField] public float RotStrength { get; private set; } = 2f;
        [field: SerializeField] public int RotVibrato { get; private set; } = 10;
    }
}