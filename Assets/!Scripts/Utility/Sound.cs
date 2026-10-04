using UnityEngine.Audio;
using UnityEngine;


[System.Serializable]
public class SoundLayer
{
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(.1f, 2f)]
    public float pitch = 1f;
}


[System.Serializable]
public class Sound
{
    public string name;
    public SoundLayer[] layers;

    [Space]
    [Range(0f, 1f)] public float volume = 1f;


    [Header("Pitch")]
    [Range(.1f, 2f)]  public float pitch = 1f;
    [Range(0f, 0.5f)] public float pitchRandomize = 0f;


    [Header("Spatial Blend")]
    public bool is3D = true;
    public bool useGlobalSpatialBlend = true;
    [Tooltip("Уникальный Spatial Blend для этого звука (работает, если отключено поле выше)")]
    [Range(0f, 1f)] public float customSpatialBlend = 1f;

    public bool isLooped;
    public AudioMixerGroup audioMixer;

    [HideInInspector]
    public AudioSource source;
}

