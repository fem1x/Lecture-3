using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "DiceUpgradeConfig", menuName = "Configs/Dice Upgrade")]
    public class DiceUpgradeConfig : ScriptableObject
    {
        [Header("Upgrade Cost")]
        [field: SerializeField, Min(0)] public int BaseCost { get; private set; } = 1;
        [field: SerializeField, Min(0)] public int CostIncrease { get; private set; } = 1;

        [Header("Bonus Points")]
        [field: SerializeField, Min(0)] public int MinBonus { get; private set; } = 5;
        [field: SerializeField, Min(0)] public int MaxBonus { get; private set; } = 15;
    }
}