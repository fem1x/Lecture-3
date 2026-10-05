using UnityEngine;
using UnityEngine.Audio;

namespace _Scripts.Scriptable_Objects.Configs
{
    [CreateAssetMenu(fileName = "AudioFilterConfig", menuName = "Configs/AudioFilterConfig")]
    public class AudioFilterConfig : ScriptableObject
    {
        [MinMaxSlider(0f, 1f)]
        [field: SerializeField] public Vector2 OnOffFilterAmount = new(0.2f, 1f);
        [field: SerializeField] public float TransitionDuration { get; private set; } = 1f;
        [field: SerializeField] public string ParameterName { get; private set; } = "Filter";
        [field: SerializeField] public AudioMixer AudioMixer { get; private set; }
    }
}