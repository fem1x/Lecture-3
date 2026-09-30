using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "RoundConfig", menuName = "Configs/RoundConfig")]
    public class ResourceConfig : ScriptableObject
    {
        [field: SerializeField] public int InitialDiceCount { get; private set; } = 15;
        [field: SerializeField] public int RerollsPerRound { get; private set; } = 2;
    }
}