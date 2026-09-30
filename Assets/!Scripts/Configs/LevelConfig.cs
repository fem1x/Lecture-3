using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/Level/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [field: SerializeField] public int ScoreQuota {get; private set;}
    }
}