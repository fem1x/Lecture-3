using System;
using _Scripts.Configs;
using _Scripts.Scriptable_Objects.Configs;
using UnityEngine;
using UnityEngine.Audio;
using VContainer.Unity;

namespace _Scripts.Managers
{
    internal enum VolumeChannel { Master, Music, Sfx, Ambience }

    public sealed class GameSettingsService : IStartable, IDisposable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private const string Prefix = "Settings.";
        private const string ShakePercentKey = Prefix + "ShakePercent";
        private const int ShakeStepPercent = 25;
        private const int MaxShakeSteps = 8;
        private static readonly string[] VolumeParameters = { "MasterVolume", "MusicVolume", "SfxVolume", "AmbienceVolume" };
        private static readonly float[] Speeds = { 0.5f, 1f, 1.25f, 1.5f, 2f };
        private static readonly FullScreenMode[] DisplayModes =
        {
            FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed, FullScreenMode.FullScreenWindow
        };

        private readonly AudioMixer _mixer;
        private readonly CameraShaker _cameraShaker;
        private readonly GameSpeedConfig _speedConfig;
        private readonly int[] _volumes = new int[VolumeParameters.Length];
        private int _speedIndex;
        private int _displayIndex;
        private bool _hasFocus;

        public bool VSync { get; private set; }
        public bool MuteWhenUnfocused { get; private set; }
        public int ShakeSteps { get; private set; }
        public int ShakePercent => ShakeSteps * ShakeStepPercent;
        public float SequenceSpeed => Speeds[_speedIndex];
        public FullScreenMode DisplayMode => DisplayModes[_displayIndex];
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public GameSettingsService(AudioFilterConfig audioConfig, CameraShaker cameraShaker, GameSpeedConfig speedConfig)
        {
            _mixer = audioConfig.AudioMixer;
            _cameraShaker = cameraShaker;
            _speedConfig = speedConfig;
        }

        public void Start()
        {
            _hasFocus = Application.isFocused;
            MuteWhenUnfocused = PlayerPrefs.GetInt(Prefix + nameof(MuteWhenUnfocused)) != 0;
            for (var i = 0; i < _volumes.Length; i++)
            {
                var defaultVolume = i == (int)VolumeChannel.Master ? 50 : 100;
                _volumes[i] = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + VolumeParameters[i], defaultVolume), 0, 100);
                ApplyVolume((VolumeChannel)i);
            }
            Application.focusChanged += HandleFocusChanged;

            VSync = PlayerPrefs.GetInt(Prefix + nameof(VSync), QualitySettings.vSyncCount > 0 ? 1 : 0) != 0;
            QualitySettings.vSyncCount = VSync ? 1 : 0;

            var savedMode = (FullScreenMode)PlayerPrefs.GetInt(Prefix + nameof(DisplayMode), (int)Screen.fullScreenMode);
            _displayIndex = Array.IndexOf(DisplayModes, savedMode);
            if (_displayIndex < 0)
                _displayIndex = Array.IndexOf(DisplayModes, FullScreenMode.FullScreenWindow);
            if (Screen.fullScreenMode != DisplayMode)
                Screen.fullScreenMode = DisplayMode;

            var legacyPercent = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + nameof(ShakeSteps), 10), 0, 20) * 10;
            var savedPercent = PlayerPrefs.GetInt(ShakePercentKey, legacyPercent);
            ShakeSteps = Mathf.RoundToInt(Mathf.Clamp(savedPercent, 0, 200) / (float)ShakeStepPercent);
            _cameraShaker.SetIntensity(ShakePercent / 100f);

            var defaultSpeedIndex = Array.IndexOf(Speeds, _speedConfig.SpeedMultiplier);
            if (defaultSpeedIndex < 0)
                defaultSpeedIndex = 1;
            _speedIndex = Mathf.Clamp(PlayerPrefs.GetInt(Prefix + nameof(SequenceSpeed), defaultSpeedIndex), 0, Speeds.Length - 1);
            _speedConfig.PlayerMultiplier = SequenceSpeed;
        }

        public void Dispose()
        {
            Application.focusChanged -= HandleFocusChanged;
            _speedConfig.PlayerMultiplier = null;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        internal int GetVolume(VolumeChannel channel) => _volumes[(int)channel];

        internal void SetVolume(VolumeChannel channel, int percent)
        {
            var index = (int)channel;
            _volumes[index] = Mathf.Clamp(percent, 0, 100);
            ApplyVolume(channel);
            PlayerPrefs.SetInt(Prefix + VolumeParameters[index], _volumes[index]);
        }

        public void SetMuteWhenUnfocused(bool enabled)
        {
            MuteWhenUnfocused = enabled;
            ApplyVolume(VolumeChannel.Master);
            PlayerPrefs.SetInt(Prefix + nameof(MuteWhenUnfocused), enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetVSync(bool enabled)
        {
            VSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            PlayerPrefs.SetInt(Prefix + nameof(VSync), enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void CycleDisplayMode()
        {
            _displayIndex = (_displayIndex + 1) % DisplayModes.Length;
            Screen.fullScreenMode = DisplayMode;
            PlayerPrefs.SetInt(Prefix + nameof(DisplayMode), (int)DisplayMode);
            PlayerPrefs.Save();
        }

        public void SetShakeSteps(int steps)
        {
            ShakeSteps = Mathf.Clamp(steps, 0, MaxShakeSteps);
            _cameraShaker.SetIntensity(ShakePercent / 100f);
            PlayerPrefs.SetInt(ShakePercentKey, ShakePercent);
        }

        public void CycleSequenceSpeed()
        {
            _speedIndex = (_speedIndex + 1) % Speeds.Length;
            _speedConfig.PlayerMultiplier = SequenceSpeed;
            PlayerPrefs.SetInt(Prefix + nameof(SequenceSpeed), _speedIndex);
            PlayerPrefs.Save();
        }

        private void HandleFocusChanged(bool hasFocus)
        {
            _hasFocus = hasFocus;
            ApplyVolume(VolumeChannel.Master);
            if (!hasFocus)
                PlayerPrefs.Save();
        }

        private void ApplyVolume(VolumeChannel channel)
        {
            var percent = GetVolume(channel);
            var muted = percent == 0 || (channel == VolumeChannel.Master && MuteWhenUnfocused && !_hasFocus);
            var decibels = muted ? -80f : 20f * Mathf.Log10(percent / 100f);
            _mixer.SetFloat(VolumeParameters[(int)channel], decibels);
        }
    }
}
