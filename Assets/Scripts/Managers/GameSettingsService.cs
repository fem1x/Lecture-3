using System;
using System.Collections.Generic;
using System.Linq;
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
        internal const int VolumeStepPercent = 5;
        private const string Prefix = "Settings.";
        private const string ShakePercentKey = Prefix + "ShakePercent";
        private const string WidthKey = Prefix + "ResolutionWidth";
        private const string HeightKey = Prefix + "ResolutionHeight";
        internal const int ShakeStepPercent = 25;
        private const int MaxShakeSteps = 8;
        private const int FallbackFpsLimit = 144;
        private const int UnlimitedFps = -1;
        private const FullScreenMode DefaultDisplayMode = FullScreenMode.FullScreenWindow;
        private static readonly string[] VolumeParameters = { "MasterVolume", "MusicVolume", "SfxVolume", "AmbienceVolume" };
        private static readonly float[] Speeds = { 0.5f, 1f, 1.25f, 1.5f, 2f };
        private static readonly int[] FpsLimitPresets = { 30, 60, 120, 144, 240 };
        private static readonly FullScreenMode[] DisplayModes =
        {
            FullScreenMode.ExclusiveFullScreen, FullScreenMode.Windowed, FullScreenMode.FullScreenWindow
        };

        private readonly AudioMixer _mixer;
        private readonly CameraShaker _cameraShaker;
        private readonly GameSpeedConfig _speedConfig;
        private readonly int[] _volumes = new int[VolumeParameters.Length];
        private readonly int[] _savedVolumes = new int[VolumeParameters.Length];
        private bool _savedMuteWhenUnfocused;
        private int[] _fpsLimits;
        private int _speedIndex;
        private int _displayIndex;
        private bool _hasFocus;

        public bool VSync { get; private set; }
        internal IReadOnlyList<int> FpsLimits => _fpsLimits;
        internal int FpsLimitIndex { get; private set; }
        internal static IReadOnlyList<float> SequenceSpeeds { get; } = Array.AsReadOnly(Speeds);
        internal static IReadOnlyList<FullScreenMode> ScreenModes { get; } = Array.AsReadOnly(DisplayModes);
        internal int SpeedIndex => _speedIndex;
        internal int DisplayIndex => _displayIndex;
        internal static bool IsFpsLimitAvailable(bool vSync) => !vSync || UsesMobileFramePacing;
        internal bool HasSoundChanges => MuteWhenUnfocused != _savedMuteWhenUnfocused ||
            !_volumes.SequenceEqual(_savedVolumes);
        private int FpsLimit => _fpsLimits[FpsLimitIndex];
        private static bool UsesMobileFramePacing =>
            Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer;
        public bool MuteWhenUnfocused { get; private set; }
        public int ShakeSteps { get; private set; }
        public int ShakePercent => ShakeSteps * ShakeStepPercent;
        public float SequenceSpeed => Speeds[_speedIndex];
        public FullScreenMode DisplayMode => DisplayModes[_displayIndex];
        public IReadOnlyList<Vector2Int> Resolutions { get; private set; }
        public int ResolutionIndex { get; private set; }
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
            MuteWhenUnfocused = PlayerPrefs.GetInt(Prefix + nameof(MuteWhenUnfocused), 1) != 0;
            for (var i = 0; i < _volumes.Length; i++)
            {
                var defaultVolume = i == (int)VolumeChannel.Master ? 50 : 100;
                _volumes[i] = SnapVolume(PlayerPrefs.GetInt(Prefix + VolumeParameters[i], defaultVolume));
                ApplyVolume((VolumeChannel)i);
            }
            Array.Copy(_volumes, _savedVolumes, _volumes.Length);
            _savedMuteWhenUnfocused = MuteWhenUnfocused;
            Application.focusChanged += HandleFocusChanged;

            InitializeDisplaySettings();

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

        internal void PreviewVolume(VolumeChannel channel, int percent)
        {
            var index = (int)channel;
            _volumes[index] = SnapVolume(percent);
            ApplyVolume(channel);
        }

        internal void SaveSound()
        {
            for (var i = 0; i < _volumes.Length; i++)
                PlayerPrefs.SetInt(Prefix + VolumeParameters[i], _volumes[i]);
            PlayerPrefs.SetInt(Prefix + nameof(MuteWhenUnfocused), MuteWhenUnfocused ? 1 : 0);
            Array.Copy(_volumes, _savedVolumes, _volumes.Length);
            _savedMuteWhenUnfocused = MuteWhenUnfocused;
        }

        internal void RestoreSound()
        {
            if (!HasSoundChanges) return;

            MuteWhenUnfocused = _savedMuteWhenUnfocused;
            Array.Copy(_savedVolumes, _volumes, _volumes.Length);
            for (var i = 0; i < _volumes.Length; i++)
                ApplyVolume((VolumeChannel)i);
        }

        private static int SnapVolume(int percent) =>
            Mathf.RoundToInt(Mathf.Clamp(percent, 0, 100) / (float)VolumeStepPercent) * VolumeStepPercent;

        internal void PreviewMuteWhenUnfocused(bool enabled)
        {
            MuteWhenUnfocused = enabled;
            ApplyVolume(VolumeChannel.Master);
        }

        internal void SetVSync(bool enabled)
        {
            if (VSync == enabled) return;

            VSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            PlayerPrefs.SetInt(Prefix + nameof(VSync), enabled ? 1 : 0);
        }

        internal void SetFpsLimit(int index)
        {
            if (FpsLimitIndex == index) return;

            FpsLimitIndex = index;
            ApplyFpsLimit();
            PlayerPrefs.SetInt(Prefix + nameof(FpsLimit), FpsLimit);
        }

        private void ApplyFpsLimit()
        {
            // A target of -1 selects Unity's 30 FPS default on mobile.
            Application.targetFrameRate = FpsLimit == UnlimitedFps && UsesMobileFramePacing
                ? Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value)
                : FpsLimit;
        }

        internal void SetDisplay(int modeIndex, int resolutionIndex)
        {
            if (_displayIndex == modeIndex && ResolutionIndex == resolutionIndex) return;

            _displayIndex = modeIndex;
            ResolutionIndex = resolutionIndex;
            ApplyResolution();
            var size = Resolutions[resolutionIndex];
            PlayerPrefs.SetInt(Prefix + nameof(DisplayMode), (int)DisplayMode);
            PlayerPrefs.SetInt(WidthKey, size.x);
            PlayerPrefs.SetInt(HeightKey, size.y);
        }

        private void InitializeDisplaySettings()
        {
            var (monitorSize, refreshRate) = GetMonitorSettings();
            var defaultFps = refreshRate.numerator > 0 && refreshRate.denominator > 0
                ? Mathf.Max(1, Mathf.RoundToInt((float)refreshRate.value))
                : FallbackFpsLimit;
            var savedFps = PlayerPrefs.GetInt(Prefix + nameof(FpsLimit), defaultFps);
            if (savedFps <= 0 && savedFps != UnlimitedFps)
                savedFps = defaultFps;

            _fpsLimits = FpsLimitPresets
                .Append(defaultFps)
                .Append(savedFps)
                .Where(fps => fps > 0)
                .Distinct()
                .OrderBy(fps => fps)
                .Append(UnlimitedFps)
                .ToArray();
            FpsLimitIndex = Array.IndexOf(_fpsLimits, savedFps);

            VSync = PlayerPrefs.GetInt(Prefix + nameof(VSync), 0) != 0;
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            ApplyFpsLimit();

            var savedMode = (FullScreenMode)PlayerPrefs.GetInt(Prefix + nameof(DisplayMode), (int)DefaultDisplayMode);
            _displayIndex = Array.IndexOf(DisplayModes, savedMode);
            if (_displayIndex < 0)
                _displayIndex = Array.IndexOf(DisplayModes, DefaultDisplayMode);
            InitializeResolution(monitorSize);
        }

        private static (Vector2Int Size, RefreshRate RefreshRate) GetMonitorSettings()
        {
#if UNITY_STANDALONE && !UNITY_EDITOR
            var monitor = Screen.mainWindowDisplayInfo;
            if (monitor.width > 0 && monitor.height > 0)
                return (new Vector2Int(monitor.width, monitor.height), monitor.refreshRate);
#endif
            var resolution = Screen.currentResolution;
            return (new Vector2Int(resolution.width, resolution.height), resolution.refreshRateRatio);
        }

        private void InitializeResolution(Vector2Int monitorSize)
        {
            var current = new Vector2Int(Screen.width, Screen.height);
            var options = Screen.resolutions
                .Select(mode => new Vector2Int(mode.width, mode.height))
                .Append(current)
                .Append(monitorSize)
                .Distinct()
                .OrderByDescending(size => size.x)
                .ThenByDescending(size => size.y)
                .ToArray();
            Resolutions = options;

            var saved = new Vector2Int(PlayerPrefs.GetInt(WidthKey, monitorSize.x), PlayerPrefs.GetInt(HeightKey, monitorSize.y));
            ResolutionIndex = Array.IndexOf(options, saved);
            if (ResolutionIndex < 0)
                ResolutionIndex = Array.IndexOf(options, monitorSize);

            if (options[ResolutionIndex] != current || Screen.fullScreenMode != DisplayMode)
                ApplyResolution();
        }

        private void ApplyResolution()
        {
            var size = Resolutions[ResolutionIndex];
            Screen.SetResolution(size.x, size.y, DisplayMode);
        }

        internal void SetShakeSteps(int steps)
        {
            steps = Mathf.Clamp(steps, 0, MaxShakeSteps);
            if (ShakeSteps == steps) return;

            ShakeSteps = steps;
            _cameraShaker.SetIntensity(ShakePercent / 100f);
            PlayerPrefs.SetInt(ShakePercentKey, ShakePercent);
        }

        internal void SetSequenceSpeed(int index)
        {
            if (_speedIndex == index) return;

            _speedIndex = index;
            _speedConfig.PlayerMultiplier = SequenceSpeed;
            PlayerPrefs.SetInt(Prefix + nameof(SequenceSpeed), _speedIndex);
        }

        private void HandleFocusChanged(bool hasFocus)
        {
            _hasFocus = hasFocus;
            ApplyVolume(VolumeChannel.Master);
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
