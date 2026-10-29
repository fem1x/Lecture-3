using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "ColorsConfig", menuName = "Configs/Colors", order = 0)]
    public class ColorsConfig : ScriptableObject
    {
        [field: SerializeField] public Color Success { get; set; } = Color.green;
        [field: SerializeField] public Color Failure { get; set; } = Color.red;
    }
}