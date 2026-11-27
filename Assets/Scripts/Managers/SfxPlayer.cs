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
        private readonly Camera _camera;
        public SfxPlayer(SfxConfig config, Camera camera)
        {
            _config = config;
            _camera = camera;
        }
        
        private readonly Dictionary<string, AudioSource> _2dSources = new();
        private readonly Dictionary<string, Sound> _soundLookup = new();
        private readonly List<(AudioSource Source, double ReadyAt)> _spatialPool = new();
        private readonly List<(AudioSource Source, double ReadyAt)> _extraLayerPool = new();
        private GameObject _root;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        public void Initialize()
        {
            _root = new GameObject("[SFXManager_Root]");
            foreach (Sound s in _config.Sounds)
            {
                if (string.IsNullOrEmpty(s.name)) continue;
                _soundLookup[s.name] = s;
                
                if (!s.is3D)
                {
                    var source = _root.AddComponent<AudioSource>();
                
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

    public void PlayUI(string name, RectTransform element)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (!_soundLookup.TryGetValue(name, out Sound sound))
        {
            Debug.LogWarning($"Sound with name '{name}' not found!");
            return;
        }

        if (!sound.is3D)
        {
            Play(name);
            return;
        }

        var canvas = element.GetComponentInParent<Canvas>().rootCanvas;
        var canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        var screenPosition = RectTransformUtility.WorldToScreenPoint(canvasCamera, element.TransformPoint(element.rect.center));
        screenPosition.x = Mathf.Clamp(screenPosition.x, 0f, Screen.width);
        screenPosition.y = Mathf.Clamp(screenPosition.y, 0f, Screen.height);
        var worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 1f));
        Play(name, worldPosition);
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

            PlayPooledLayer(s, layer, layerClip, baseRandomPitch, spawnPos);
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
            if (s.allowOverlap && !s.isLooped)
            {
                PlayPooledLayer(s, firstLayer, firstClip, baseRandomPitch);
            }
            else
            {
                mainSource.clip = firstClip;
                mainSource.volume = s.volume * firstLayer.volume;
                mainSource.pitch = Mathf.Clamp(baseRandomPitch * firstLayer.pitch, 0.1f, 3f);
                mainSource.Play();
            }
        }

        for (int i = 1; i < s.layers.Length; i++)
        {
            var layer = s.layers[i];
            AudioClip layerClip = GetRandomClip(layer);
            if (layerClip == null) continue;

            PlayPooledLayer(s, layer, layerClip, baseRandomPitch);
        }
    }

    private void PlayPooledLayer(Sound sound, SoundLayer layer, AudioClip clip, float pitch, Vector3 position = default)
    {
        var pool = sound.is3D ? _spatialPool : _extraLayerPool;
        var now = AudioSettings.dspTime;
        var freeIndex = 0;
        for (; freeIndex < pool.Count; freeIndex++)
        {
            var voice = pool[freeIndex];
            if (now >= voice.ReadyAt && !voice.Source.isPlaying)
                break;
        }

        var source = freeIndex < pool.Count
            ? pool[freeIndex].Source
            : sound.is3D
                ? Object.Instantiate(_config.SfxObject, position, Quaternion.identity, _root.transform)
                : _root.AddComponent<AudioSource>();

        if (sound.is3D)
            source.transform.position = position;

        ConfigureAudioSource(source, sound, layer, clip, pitch);
        source.Play();

        // Reserve the voice until playback has had time to start and finish on the audio clock.
        var readyAt = source.loop ? double.PositiveInfinity : AudioSettings.dspTime + clip.length / source.pitch;
        if (freeIndex == pool.Count)
            pool.Add((source, readyAt));
        else
            pool[freeIndex] = (source, readyAt);
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
