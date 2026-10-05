using System;
using System.Collections.Generic;
using _Scripts.Scriptable_Objects.Configs;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Scripts.Managers
{
    public class SfxPlayer : ISfxPlayer
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly SfxConfig _config;
        public SfxPlayer(SfxConfig config)
        {
            _config = config;
        }
        
        private Dictionary<string, AudioSource> _2dSources = new();
        private Dictionary<string, Sound> _soundLookup = new();
        private GameObject _root2D;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        public void Initialize()
        {
            _root2D = new GameObject("[SFXManager_2D_Root]");
            foreach (Sound s in _config.Sounds)
            {
                if (string.IsNullOrEmpty(s.name)) continue;
                _soundLookup[s.name] = s;
                
                if (!s.is3D)
                {
                    var source = _root2D.AddComponent<AudioSource>();
                
                    if (s.layers != null && s.layers.Length > 0)
                    {
                        AudioClip firstClip = GetRandomClip(s.layers[0]);
                        if (firstClip != null) source.clip = firstClip;
                    }

                    source.volume = s.volume;
                    source.pitch = s.pitch;
                    source.spatialBlend = GetSpatialBlend(s);
                    source.loop = s.isLooped;
                    source.outputAudioMixerGroup = s.audioMixer;

                    _2dSources[s.name] = source;
                }
            }
        }
        
    /// <summary>
    /// Use for 3D sounds with random pitch
    /// </summary>
    public void Play(string name, Vector3 spawnPos, float pitchMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!_soundLookup.TryGetValue(name, out Sound s))        
        {
            Debug.LogWarning($"Sound with name '{name}' not found!");
            return;
        }
        if (!s.is3D)
        {
            Debug.LogException(new Exception("Using 3D sound method for 2D SFX! Don't set spawnPos parameter."));
            return;
        }

        float randomOffset = Random.Range(-s.pitchRandomize, s.pitchRandomize);
        float finalPitch = (s.pitch + randomOffset) * pitchMultiplier;
        PlayLayers(s, spawnPos, finalPitch);
    }

    /// <summary>
    /// Use for 2D sounds
    /// </summary>
    public void Play(string name, float pitchMultiplier = 1f)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!_soundLookup.TryGetValue(name, out Sound s))        
        {
            Debug.LogWarning($"Sound with name '{name}' not found!");
            return;
        }
        if (s.customSpatialBlend > 0 && s.useGlobalSpatialBlend)
            Debug.LogException(new Exception("2D sounds should have SpatialBlend = 0"));

        float randomOffset = Random.Range(-s.pitchRandomize, s.pitchRandomize);
        float finalPitch = (s.pitch + randomOffset) * pitchMultiplier;
        Play2DLayers(s, finalPitch);
    }

    //===== ===== ===== ===== ===== ===== ===== ===== ===== =====

    #region Play Layers

    private void PlayLayers(Sound s, Vector3 spawnPos, float baseRandomPitch)
    {
        if (s.layers == null || s.layers.Length == 0) return;

        foreach (var layer in s.layers)
        {
            AudioClip layerClip = GetRandomClip(layer);
            if (layerClip == null) continue;

            AudioSource layerSource = Object.Instantiate(_config.SfxObject, spawnPos, Quaternion.identity);

            ConfigureAudioSource(layerSource, s, layer, layerClip, baseRandomPitch);
            layerSource.Play();

            if (!layerSource.loop)
            {
                float length = layerClip.length / Mathf.Abs(layerSource.pitch);
                Object.Destroy(layerSource.gameObject, length);
            }
        }
    }

    private void Play2DLayers(Sound s, float baseRandomPitch)
    {
        if (s.layers == null || s.layers.Length == 0) return;
        if (!_2dSources.TryGetValue(s.name, out AudioSource mainSource)) return;
        
        var firstLayer = s.layers[0];
        AudioClip firstClip = GetRandomClip(firstLayer);
        if (firstClip != null)
        {
            mainSource.clip = firstClip;
            mainSource.volume = s.volume * firstLayer.volume;

            float pitchRatio = firstLayer.pitch / s.pitch;
            mainSource.pitch = Mathf.Clamp(baseRandomPitch * pitchRatio, 0.1f, 3f);
            mainSource.Play();
        }

        for (int i = 1; i < s.layers.Length; i++)
        {
            var layer = s.layers[i];
            AudioClip layerClip = GetRandomClip(layer);
            if (layerClip == null) continue;

            AudioSource layerSource = _root2D.AddComponent<AudioSource>();
            ConfigureAudioSource(layerSource, s, layer, layerClip, baseRandomPitch);
            layerSource.Play();

            if (!layerSource.loop)
            {
                float length = layerClip.length / Mathf.Abs(layerSource.pitch);
                Object.Destroy(layerSource, length);
            }
        }
    }
    #endregion


    #region Helper Methods
    private void ConfigureAudioSource(AudioSource source, Sound s, SoundLayer layer, AudioClip clip, float baseRandomPitch)
    {
        source.clip = clip;
        source.volume = s.volume * layer.volume;

        source.pitch = Mathf.Clamp(baseRandomPitch * layer.pitch, 0.1f, 3f);
        
        source.spatialBlend = GetSpatialBlend(s);
        source.loop = s.isLooped;
        source.outputAudioMixerGroup = s.audioMixer;
    }

    private AudioClip GetRandomClip(SoundLayer layer)
    {
        if (layer.clips == null || layer.clips.Length == 0) return null;
        int randomIndex = Random.Range(0, layer.clips.Length);
        return layer.clips[randomIndex];
    }

    private float GetSpatialBlend(Sound s)
    {
        return s.useGlobalSpatialBlend ? _config.GlobalSpatialBlend : s.customSpatialBlend;
    }
    #endregion
    }
}