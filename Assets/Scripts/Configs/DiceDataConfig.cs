using _Scripts.Dices;
using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "DiceDataConfig", menuName = "Configs/DiceDataConfig")]
    public class DiceDataConfig : ScriptableObject
    {
        [field: SerializeField] public string DiceName { get; private set; } = "Standard";
        [field: SerializeField] public int MaxDurability { get; private set; } = 2;
        
        [field: SerializeField] public DiceFace[] Faces { get; private set; } = 
        {
            new(1, Vector3.up),
            new(6, Vector3.down),
            new(5, Vector3.right),
            new(2, Vector3.left),
            new(3, Vector3.forward),
            new(4, Vector3.back)
        };
    }
}