using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "DiceRollConfig", menuName = "Configs/DiceRollConfig")]
    public class DiceRollConfig : ScriptableObject
    {
        [Header("Throw Presets")]
        [field: SerializeField] public DiceThrowSettings FirstRollSettings { get; private set; } = new()
        {
            Direction = new Vector3(0f, 0.65f, 1f),
            DirectionRandomize = 0f,
            Force = 4.1f,
            ForceRandomize = 0.5f,
            Torque = 12f,
            TorqueRandomize = 2f,
            TimeBetweenThrows = 0.2f
        };

        [field: SerializeField] public DiceThrowSettings RerollSettings { get; private set; } = new()
        {
            Direction = Vector3.up,
            DirectionRandomize = 0.15f,
            Force = 3f,
            ForceRandomize = 0.5f,
            Torque = 12f,
            TorqueRandomize = 2f,
            TimeBetweenThrows = 0.1f
        };
    }
}