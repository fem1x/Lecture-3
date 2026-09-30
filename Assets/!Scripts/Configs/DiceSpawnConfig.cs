using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "DiceSpawnConfig", menuName = "Configs/DiceSpawnConfig")]
    public class DiceSpawnConfig : ScriptableObject
    {
        [field: SerializeField] public Dice DicePrefab { get; private set; }
        [field: SerializeField] public Transform BasePosition { get; private set; }
        [field: SerializeField] public float Spacing { get; private set; } = 2f;
        [field: SerializeField] public bool RandomizeStartingRotation { get; private set; } = true;
    }
}