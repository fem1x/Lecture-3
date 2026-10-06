using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "TokenRewardConfig", menuName = "Configs/TokenRewardConfig")]
    public class TokenRewardConfig : ScriptableObject
    {
        [field: SerializeField] public int ThrowsPerToken { get; set; } = 5;
        [field: SerializeField] public int RerollsPerToken { get; set; } = 2;
    }
}