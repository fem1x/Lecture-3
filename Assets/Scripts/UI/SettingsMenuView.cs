using System;
using System.Threading;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public sealed class SettingsMenuView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Serializable]
        private sealed class Tab
        {
            public Button Button;
            public Image Accent;
            public CanvasGroup Page;
        }

        [Serializable]
        private sealed class SliderField
        {
            public Slider Slider;
            public TMP_Text Value;
        }

        private const string LocalePreference = "Settings.Locale";
        private static readonly Color ActiveTabColor = new Color32(85, 57, 106, 255);
        private static readonly Color InactiveTabColor = new Color32(45, 35, 55, 255);
        private static readonly Color OffSwitchColor = new Color32(125, 27, 50, 255);

        [SerializeField] private SlidingPanelView _slide;
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private CanvasGroup _backdrop;
        [SerializeField] private Button _backdropButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Tab[] _tabs;

        [Header("Backdrop")]
        [SerializeField, Min(0f)] private float _backdropFadeInTime = 0.22f;
        [SerializeField, Min(0f)] private float _backdropFadeOutTime = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _backdropOpacity = 0.62f;

        [Header("Sound")]
        [SerializeField] private SliderField[] _volumeFields;
        [SerializeField] private Button _muteButton;
        [SerializeField] private LocalizeStringEvent _muteValue;
        [SerializeField] private TMP_Text _muteLabel;

        [Header("Display")]
        [SerializeField] private Button _vSyncButton;
        [SerializeField] private LocalizeStringEvent _vSyncValue;
        [SerializeField] private TMP_Text _vSyncLabel;
        [SerializeField] private Button _displayButton;
        [SerializeField] private LocalizeStringEvent _displayValue;

        [Header("Game")]
        [SerializeField] private Button _languageButton;
        [SerializeField] private TMP_Text _languageValue;
        [SerializeField] private Locale[] _locales;
        [SerializeField] private SliderField _shakeField;
        [SerializeField] private Button _speedButton;
        [SerializeField] private TMP_Text _speedValue;

        private GameSettingsService _settings;
        private UnityAction<float>[] _volumeListeners;
        private CancellationTokenSource _transition;
        private Tween _backdropTween;
        private Tween _tabTween;
        private Tween _muteTween;
        private Tween _vSyncTween;
        private GameObject _previousSelection;
        private int _activeTab;
        private bool _wantsOpen;

        public static bool BlocksGameplayInput { get; private set; }

        [Inject]
        public void Construct(GameSettingsService settings) => _settings = settings;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _volumeListeners = new UnityAction<float>[_volumeFields.Length];
            for (var i = 0; i < _volumeListeners.Length; i++)
            {
                var channel = (VolumeChannel)i;
                _volumeListeners[i] = value => ChangeVolume(channel, (int)value);
            }
        }

        private void OnEnable()
        {
            _backdropButton.onClick.AddListener(Hide);
            _closeButton.onClick.AddListener(Hide);
            _tabs[0].Button.onClick.AddListener(ShowSoundTab);
            _tabs[1].Button.onClick.AddListener(ShowDisplayTab);
            _tabs[2].Button.onClick.AddListener(ShowGameTab);
            _muteButton.onClick.AddListener(ToggleMute);
            _vSyncButton.onClick.AddListener(ToggleVSync);
            _displayButton.onClick.AddListener(CycleDisplayMode);
            _languageButton.onClick.AddListener(CycleLanguage);
            _shakeField.Slider.onValueChanged.AddListener(ChangeShake);
            _speedButton.onClick.AddListener(CycleSpeed);
            for (var i = 0; i < _volumeFields.Length; i++)
                _volumeFields[i].Slider.onValueChanged.AddListener(_volumeListeners[i]);
            LocalizationSettings.SelectedLocaleChanged += UpdateLanguage;
        }

        private void OnDisable()
        {
            _backdropButton.onClick.RemoveListener(Hide);
            _closeButton.onClick.RemoveListener(Hide);
            _tabs[0].Button.onClick.RemoveListener(ShowSoundTab);
            _tabs[1].Button.onClick.RemoveListener(ShowDisplayTab);
            _tabs[2].Button.onClick.RemoveListener(ShowGameTab);
            _muteButton.onClick.RemoveListener(ToggleMute);
            _vSyncButton.onClick.RemoveListener(ToggleVSync);
            _displayButton.onClick.RemoveListener(CycleDisplayMode);
            _languageButton.onClick.RemoveListener(CycleLanguage);
            _shakeField.Slider.onValueChanged.RemoveListener(ChangeShake);
            _speedButton.onClick.RemoveListener(CycleSpeed);
            for (var i = 0; i < _volumeFields.Length; i++)
                _volumeFields[i].Slider.onValueChanged.RemoveListener(_volumeListeners[i]);
            LocalizationSettings.SelectedLocaleChanged -= UpdateLanguage;

            CancelTransition();
            _tabTween?.Kill();
            _muteTween?.Kill();
            _vSyncTween?.Kill();
            _wantsOpen = false;
            if (BlocksGameplayInput)
            {
                PlayerPrefs.Save();
                FinishHide();
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            if (_wantsOpen)
                Hide();
            else
                Show();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Show()
        {
            if (!isActiveAndEnabled || _wantsOpen) return;

            _wantsOpen = true;
            if (!BlocksGameplayInput)
            {
                CaptureSelection();
                RefreshControls();
                SelectTab(_activeTab, animate: false);
            }
            BlocksGameplayInput = true;
            _backdrop.blocksRaycasts = true;
            SetVisibleAsync(true).Forget();
        }

        public void Hide()
        {
            if (!_wantsOpen) return;

            _wantsOpen = false;
            _panel.interactable = false;
            _panel.blocksRaycasts = false;
            _backdrop.interactable = false;
            PlayerPrefs.Save();
            SetVisibleAsync(false).Forget();
        }

        private async UniTask SetVisibleAsync(bool visible)
        {
            CancelTransition();
            _transition = new CancellationTokenSource();
            var token = _transition.Token;
            _backdropTween = _backdrop.DOFade(visible ? _backdropOpacity : 0f,
                    visible ? _backdropFadeInTime : _backdropFadeOutTime)
                .SetEase(Ease.OutCubic).SetUpdate(true);

            if (visible)
            {
                var slide = _slide.ShowAsync(token);
                _panel.interactable = false;
                _panel.blocksRaycasts = false;
                if (await slide.SuppressCancellationThrow() || token.IsCancellationRequested) return;

                _panel.interactable = true;
                _panel.blocksRaycasts = true;
                _backdrop.interactable = true;
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(_tabs[_activeTab].Button.gameObject);
            }
            else
            {
                await UniTask.WhenAll(
                    _slide.HideAsync(token).SuppressCancellationThrow(),
                    _backdropTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token).SuppressCancellationThrow());
                if (token.IsCancellationRequested) return;
                FinishHide();
            }
        }

        private void CancelTransition()
        {
            _transition?.Cancel();
            _transition?.Dispose();
            _transition = null;
            _backdropTween?.Kill();
        }

        private void CaptureSelection()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return;
            _previousSelection = eventSystem.currentSelectedGameObject;
            eventSystem.SetSelectedGameObject(null);
        }

        private void FinishHide()
        {
            _panel.alpha = 0f;
            _panel.interactable = false;
            _panel.blocksRaycasts = false;
            _backdrop.alpha = 0f;
            _backdrop.interactable = false;
            _backdrop.blocksRaycasts = false;
            BlocksGameplayInput = false;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_previousSelection != null && _previousSelection.activeInHierarchy
                    ? _previousSelection
                    : null);
            _previousSelection = null;
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void ShowSoundTab() => SelectTab(0, animate: true);
        private void ShowDisplayTab() => SelectTab(1, animate: true);
        private void ShowGameTab() => SelectTab(2, animate: true);

        private void SelectTab(int index, bool animate)
        {
            if (animate && _activeTab == index) return;

            _tabTween?.Kill();
            _activeTab = index;
            for (var i = 0; i < _tabs.Length; i++)
            {
                var selected = i == index;
                var tab = _tabs[i];
                tab.Button.image.color = selected ? ActiveTabColor : InactiveTabColor;
                tab.Accent.enabled = selected;
                tab.Page.gameObject.SetActive(selected);
            }

            var page = _tabs[index].Page;
            var rect = (RectTransform)page.transform;
            page.alpha = animate ? 0f : 1f;
            rect.anchoredPosition = animate ? Vector2.down * 10f : Vector2.zero;
            if (animate)
                _tabTween = DOTween.Sequence().SetUpdate(true)
                    .Join(page.DOFade(1f, 0.18f).SetEase(Ease.OutQuad))
                    .Join(rect.DOAnchorPos(Vector2.zero, 0.22f).SetEase(Ease.OutCubic));
        }

        private void RefreshControls()
        {
            for (var i = 0; i < _volumeFields.Length; i++)
            {
                var value = _settings.GetVolume((VolumeChannel)i);
                _volumeFields[i].Slider.SetValueWithoutNotify(value);
                _volumeFields[i].Value.text = $"{value}%";
            }

            _shakeField.Slider.SetValueWithoutNotify(_settings.ShakeSteps);
            _shakeField.Value.text = $"{_settings.ShakePercent}%";
            _muteTween?.Kill();
            _vSyncTween?.Kill();
            SetSwitchInstant(_muteButton, _muteValue, _muteLabel, _settings.MuteWhenUnfocused);
            SetSwitchInstant(_vSyncButton, _vSyncValue, _vSyncLabel, _settings.VSync);
            RefreshDisplayMode();
            UpdateLanguage(LocalizationSettings.SelectedLocale);
        }

        private void ChangeVolume(VolumeChannel channel, int value)
        {
            _settings.SetVolume(channel, value);
            _volumeFields[(int)channel].Value.text = $"{value}%";
        }

        private void ChangeShake(float steps)
        {
            _settings.SetShakeSteps((int)steps);
            _shakeField.Value.text = $"{_settings.ShakePercent}%";
        }

        private void ToggleMute()
        {
            _settings.SetMuteWhenUnfocused(!_settings.MuteWhenUnfocused);
            _muteTween?.Kill();
            _muteTween = AnimateSwitch(_muteButton, _muteValue, _muteLabel, _settings.MuteWhenUnfocused);
        }

        private void ToggleVSync()
        {
            _settings.SetVSync(!_settings.VSync);
            _vSyncTween?.Kill();
            _vSyncTween = AnimateSwitch(_vSyncButton, _vSyncValue, _vSyncLabel, _settings.VSync);
        }

        private void CycleDisplayMode()
        {
            _settings.CycleDisplayMode();
            RefreshDisplayMode();
        }

        private void CycleSpeed()
        {
            _settings.CycleSequenceSpeed();
            UpdateSpeedLabel(LocalizationSettings.SelectedLocale);
        }

        private static void SetSwitchInstant(Button button, LocalizeStringEvent value, TMP_Text label, bool enabled)
        {
            button.image.color = enabled ? ActiveTabColor : OffSwitchColor;
            value.SetEntry(enabled ? "settings_on" : "settings_off");
            label.alpha = 1f;
        }

        private static Tween AnimateSwitch(Button button, LocalizeStringEvent value, TMP_Text label, bool enabled)
        {
            return DOTween.Sequence().SetUpdate(true)
                .Join(button.image.DOColor(enabled ? ActiveTabColor : OffSwitchColor, 0.22f).SetEase(Ease.OutCubic))
                .Join(DOTween.Sequence()
                    .Append(DOTween.To(() => label.alpha, alpha => label.alpha = alpha, 0f, 0.08f))
                    .AppendCallback(() => value.SetEntry(enabled ? "settings_on" : "settings_off"))
                    .Append(DOTween.To(() => label.alpha, alpha => label.alpha = alpha, 1f, 0.14f)));
        }

        private void RefreshDisplayMode()
        {
            _displayValue.SetEntry(_settings.DisplayMode switch
            {
                FullScreenMode.ExclusiveFullScreen => "settings_fullscreen",
                FullScreenMode.Windowed => "settings_windowed",
                _ => "settings_borderless"
            });
        }

        private void CycleLanguage()
        {
            var index = Array.IndexOf(_locales, LocalizationSettings.SelectedLocale);
            var locale = _locales[(index + 1) % _locales.Length];
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocalePreference, locale.Identifier.Code);
            PlayerPrefs.Save();
        }

        private void UpdateLanguage(Locale locale)
        {
            var culture = locale?.Identifier.CultureInfo;
            var name = culture?.NativeName ?? locale?.LocaleName ?? string.Empty;
            _languageValue.text = name.Length == 0
                ? name
                : (culture?.TextInfo.ToUpper(name[0]) ?? char.ToUpperInvariant(name[0])) + name.Substring(1);
            if (_settings != null)
                UpdateSpeedLabel(locale);
        }

        private void UpdateSpeedLabel(Locale locale) =>
            _speedValue.text = "×" + _settings.SequenceSpeed.ToString("0.##", locale?.Identifier.CultureInfo);
    }
}
