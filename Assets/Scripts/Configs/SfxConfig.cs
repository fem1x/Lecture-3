using UnityEngine;

namespace _Scripts.Scriptable_Objects.Configs
{
    [CreateAssetMenu(fileName = "SfxConfig", menuName = "Configs/SfxConfig")]
    public class SfxConfig : ScriptableObject
    {
        [field: SerializeField] public AudioSource SfxObject {get; private set;}
        
        [Header("Global Audio Settings")]
        [Range(0f, 1f)]
        [field: SerializeField] public float GlobalSpatialBlend {get; private set;} = 1f;

        [Header("SFX Library")]
        [field: SerializeField] public Sound[] Sounds {get; private set;}

    }
}