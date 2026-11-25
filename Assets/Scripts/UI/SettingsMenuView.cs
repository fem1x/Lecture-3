using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            public Vector2 ShownPosition { get; set; }
            public Color BaseColor { get; set; }
            public SettingsTabAccentView AccentView { get; set; }
        }

        [Serializable]
        private sealed class SliderField
        {
            public Slider Slider;
            public TMP_Text Value;
        }

        private sealed class EntranceItem
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public Vector3 Scale;
            public float Alpha;
        }

        private const string LocalePreference = "Settings.Locale";

        [SerializeField] private SlidingPanelView _slide;
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private CanvasGroup _backdrop;
        [SerializeField] private Button _backdropButton;
        [SerializeField] private Button _closeButton;

        [Header("Tabs")]
        [SerializeField] private Tab[] _tabs;
        [SerializeField, Range(0f, 1f)] private float _inactiveTabDarkening = 0.2f;

        [Header("Backdrop")]
        [SerializeField, Min(0f)] private float _backdropFadeInTime = 0.22f;
        [SerializeField, Min(0f)] private float _backdropFadeOutTime = 0.2f;
        [SerializeField] private Ease _backdropFadeInEase = Ease.OutCubic;
        [SerializeField] private Ease _backdropFadeOutEase = Ease.OutCubic;
        [SerializeField, Range(0f, 1f)] private float _backdropOpacity = 0.62f;

        [Header("Sound")]
        [SerializeField] private SliderField[] _volumeFields;
        [SerializeField] private Toggle _muteToggle;
        [SerializeField] private CheckboxFeedbackView _muteFeedback;

        [Header("Display")]
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private CheckboxFeedbackView _vSyncFeedback;
        [SerializeField] private Button _displayButton;
        [SerializeField] private LocalizeStringEvent _displayValue;
        [SerializeField] private TMP_Text _displayLabel;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Text _resolutionValue;

        [Header("Game")]
        [SerializeField] private TMP_Dropdown _languageDropdown;
        [SerializeField] private TMP_Text _languageValue;
        [SerializeField] private Locale[] _locales;
        [SerializeField] private SliderField _shakeField;
        [SerializeField] private Button _speedButton;
        [SerializeField] private TMP_Text _speedValue;

        private GameSettingsService _settings;
        private ISfxPlayer _sfxPlayer;
        private UnityAction<float>[] _volumeListeners;
        private CancellationTokenSource _transition;
        private Tween _backdropTween;
        private Tween _tabTween;
        private Sequence _entranceTween;
        private TMP_Text _title;
        private int _titleVisibleCharacters;
        private EntranceItem _closeEntrance;
        private EntranceItem[] _tabEntrances;
        private EntranceItem[][] _pageEntrances;
        private readonly Dictionary<TMP_Text, (Vector2 Position, Vector3 Scale)> _valueRestPoses = new();
        private GameObject _previousSelection;
        private int _activeTab;
        private bool _wantsOpen;

        public static bool BlocksGameplayInput { get; private set; }

        [Inject]
        public void Construct(GameSettingsService settings, ISfxPlayer sfxPlayer)
        {
            _settings = settings;
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            foreach (var tab in _tabs)
            {
                tab.ShownPosition = ((RectTransform)tab.Page.transform).anchoredPosition;
                tab.BaseColor = tab.Button.image.color;
                tab.AccentView = tab.Accent.GetComponent<SettingsTabAccentView>();
            }

            _title = _panel.transform.Find("(Panel) Header/(Text) PanelName").GetComponent<TMP_Text>();
            _titleVisibleCharacters = _title.maxVisibleCharacters;
            _closeEntrance = CreateEntranceItem((RectTransform)_closeButton.transform);
            _tabEntrances = _tabs.Select(tab => CreateEntranceItem((RectTransform)tab.Button.transform)).ToArray();
            _pageEntrances = _tabs.Select(tab =>
                tab.Page.transform.Cast<Transform>().Select(child => CreateEntranceItem((RectTransform)child)).ToArray()).ToArray();

            foreach (var label in new[] { _displayLabel, _resolutionValue, _languageValue, _speedValue })
                _valueRestPoses.Add(label, (label.rectTransform.anchoredPosition, label.rectTransform.localScale));

            _volumeListeners = new UnityAction<float>[_volumeFields.Length];
            for (var i = 0; i < _volumeListeners.Length; i++)
            {
                var channel = (VolumeChannel)i;
                _volumeListeners[i] = step => ChangeVolume(channel, (int)step * GameSettingsService.VolumeStepPercent);
            }
        }

        private void OnEnable()
        {
            _backdropButton.onClick.AddListener(Hide);
            _closeButton.onClick.AddListener(Hide);
            _tabs[0].Button.onClick.AddListener(ShowGameTab);
            _tabs[1].Button.onClick.AddListener(ShowDisplayTab);
            _tabs[2].Button.onClick.AddListener(ShowSoundTab);
            _muteToggle.onValueChanged.AddListener(ToggleMute);
            _vSyncToggle.onValueChanged.AddListener(ToggleVSync);
            _displayButton.onClick.AddListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.AddListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.AddListener(PreviousDisplayMode);
            _resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
            _languageDropdown.onValueChanged.AddListener(ChangeLanguage);
            _shakeField.Slider.onValueChanged.AddListener(ChangeShake);
            _speedButton.onClick.AddListener(CycleSpeed);
            _speedButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.AddListener(CycleSpeed);
            _speedButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.AddListener(PreviousSpeed);
            for (var i = 0; i < _volumeFields.Length; i++)
                _volumeFields[i].Slider.onValueChanged.AddListener(_volumeListeners[i]);
            LocalizationSettings.SelectedLocaleChanged += UpdateLanguage;
        }

        private void OnDisable()
        {
            _backdropButton.onClick.RemoveListener(Hide);
            _closeButton.onClick.RemoveListener(Hide);
            _tabs[0].Button.onClick.RemoveListener(ShowGameTab);
            _tabs[1].Button.onClick.RemoveListener(ShowDisplayTab);
            _tabs[2].Button.onClick.RemoveListener(ShowSoundTab);
            _muteToggle.onValueChanged.RemoveListener(ToggleMute);
            _vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
            _displayButton.onClick.RemoveListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.RemoveListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.RemoveListener(PreviousDisplayMode);
            _resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
            _languageDropdown.onValueChanged.RemoveListener(ChangeLanguage);
            _shakeField.Slider.onValueChanged.RemoveListener(ChangeShake);
            _speedButton.onClick.RemoveListener(CycleSpeed);
            _speedButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.RemoveListener(CycleSpeed);
            _speedButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.RemoveListener(PreviousSpeed);
            for (var i = 0; i < _volumeFields.Length; i++)
                _volumeFields[i].Slider.onValueChanged.RemoveListener(_volumeListeners[i]);
            LocalizationSettings.SelectedLocaleChanged -= UpdateLanguage;

            CancelTransition();
            _tabTween?.Kill();
            StopValueAnimations();
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
            if (_resolutionDropdown.IsExpanded)
                _resolutionDropdown.Hide();
            if (_languageDropdown.IsExpanded)
                _languageDropdown.Hide();
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
                .SetEase(visible ? _backdropFadeInEase : _backdropFadeOutEase).SetUpdate(true);

            if (visible)
            {
                var slide = _slide.ShowAsync(token);
                PlayEntrance();
                _backdrop.interactable = true;
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(_tabs[_activeTab].Button.gameObject);
                await UniTask.WhenAll(
                    slide.SuppressCancellationThrow(),
                    _entranceTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token).SuppressCancellationThrow());
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
            ResetEntrance();
        }

        private static EntranceItem CreateEntranceItem(RectTransform rect)
        {
            var group = rect.GetComponent<CanvasGroup>();
            if (group == null)
                group = rect.gameObject.AddComponent<CanvasGroup>();

            return new EntranceItem { Rect = rect, Group = group, Scale = rect.localScale, Alpha = group.alpha };
        }

        private void PlayEntrance()
        {
            _entranceTween = DOTween.Sequence().SetUpdate(true);
            _title.ForceMeshUpdate();
            _title.maxVisibleCharacters = 0;
            var titleProgress = 0f;
            _entranceTween.Insert(0.12f, DOTween.To(() => titleProgress, value =>
            {
                titleProgress = value;
                _title.maxVisibleCharacters = Mathf.CeilToInt(value * _title.textInfo.characterCount);
            }, 1f, 0.4f).SetEase(Ease.Linear));
            _entranceTween.InsertCallback(0.52f, () => _title.maxVisibleCharacters = _titleVisibleCharacters);
            AddEntrance(_closeEntrance, 0.12f);
            for (var i = 0; i < _tabEntrances.Length; i++)
                AddEntrance(_tabEntrances[i], 0.2f + i * 0.055f);
            for (var i = 0; i < _pageEntrances[_activeTab].Length; i++)
                AddEntrance(_pageEntrances[_activeTab][i], 0.28f + i * 0.055f);
        }

        private void AddEntrance(EntranceItem item, float delay)
        {
            item.Group.alpha = 0f;
            item.Rect.localScale = item.Scale * 0.96f;
            _entranceTween.Insert(delay, item.Group.DOFade(item.Alpha, 0.24f).SetEase(Ease.OutQuad));
            _entranceTween.Insert(delay, item.Rect.DOScale(item.Scale, 0.32f).SetEase(Ease.OutCubic));
        }

        private void ResetEntrance()
        {
            _entranceTween?.Kill();
            _entranceTween = null;
            _title.maxVisibleCharacters = _titleVisibleCharacters;
            ResetEntranceItem(_closeEntrance);
            foreach (var item in _tabEntrances)
                ResetEntranceItem(item);
            foreach (var page in _pageEntrances)
                foreach (var item in page)
                    ResetEntranceItem(item);
        }

        private static void ResetEntranceItem(EntranceItem item)
        {
            item.Group.alpha = item.Alpha;
            item.Rect.localScale = item.Scale;
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

        private void ShowGameTab() => SelectTab(0, animate: true);
        private void ShowDisplayTab() => SelectTab(1, animate: true);
        private void ShowSoundTab() => SelectTab(2, animate: true);

        private void SelectTab(int index, bool animate)
        {
            if (animate && _activeTab == index) return;

            _tabTween?.Kill();
            _activeTab = index;
            for (var i = 0; i < _tabs.Length; i++)
            {
                var selected = i == index;
                var tab = _tabs[i];
                var brightness = selected ? 1f : 1f - _inactiveTabDarkening;
                var color = tab.BaseColor;
                tab.Button.image.color = new Color(color.r * brightness, color.g * brightness, color.b * brightness, color.a);
                tab.AccentView.SetSelected(selected, animate);
                tab.Page.gameObject.SetActive(selected);
            }

            var tabToShow = _tabs[index];
            var page = tabToShow.Page;
            var rect = (RectTransform)page.transform;
            page.alpha = animate ? 0f : 1f;
            rect.anchoredPosition = tabToShow.ShownPosition + (animate ? Vector2.down * 10f : Vector2.zero);
            if (animate)
                _tabTween = DOTween.Sequence().SetUpdate(true)
                    .Join(page.DOFade(1f, 0.18f).SetEase(Ease.OutQuad))
                    .Join(rect.DOAnchorPos(tabToShow.ShownPosition, 0.22f).SetEase(Ease.OutCubic));
        }

        private void RefreshControls()
        {
            StopValueAnimations();
            var locale = LocalizationSettings.SelectedLocale;
            for (var i = 0; i < _volumeFields.Length; i++)
            {
                var value = _settings.GetVolume((VolumeChannel)i);
                _volumeFields[i].Slider.SetValueWithoutNotify(value / GameSettingsService.VolumeStepPercent);
                _volumeFields[i].Value.text = PercentText(value);
            }

            _shakeField.Slider.SetValueWithoutNotify(_settings.ShakeSteps);
            _shakeField.Value.text = PercentText(_settings.ShakePercent);
            _muteFeedback.SetValueWithoutNotify(_settings.MuteWhenUnfocused);
            _vSyncFeedback.SetValueWithoutNotify(_settings.VSync);
            _displayValue.SetEntry(DisplayModeKey());
            _resolutionDropdown.ClearOptions();
            _resolutionDropdown.AddOptions(_settings.Resolutions.Select(ResolutionText).ToList());
            _resolutionDropdown.SetValueWithoutNotify(_settings.ResolutionIndex);
            _resolutionValue.text = _resolutionDropdown.options[_settings.ResolutionIndex].text;

            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(_locales.Select(LanguageName).ToList());
            _languageDropdown.SetValueWithoutNotify(Array.IndexOf(_locales, locale));
            _languageValue.text = LanguageName(locale);
            _speedValue.text = SpeedText();
        }

        private void ChangeVolume(VolumeChannel channel, int value)
        {
            _settings.SetVolume(channel, value);
            _volumeFields[(int)channel].Value.text = PercentText(value);
        }

        private void ChangeShake(float steps)
        {
            _settings.SetShakeSteps((int)steps);
            _shakeField.Value.text = PercentText(_settings.ShakePercent);
        }

        private void ToggleMute(bool enabled)
        {
            _settings.SetMuteWhenUnfocused(enabled);
            _sfxPlayer.Play("ButtonClick");
        }

        private void ToggleVSync(bool enabled)
        {
            _settings.SetVSync(enabled);
            _sfxPlayer.Play("ButtonClick");
        }

        private void CycleDisplayMode() => ChangeDisplayMode(1);

        private void PreviousDisplayMode() => ChangeDisplayMode(-1);

        private void ChangeDisplayMode(int direction)
        {
            _settings.CycleDisplayMode(direction);
            var key = DisplayModeKey();
            AnimateCycleValue(_displayLabel, () => _displayValue.SetEntry(key), direction);
        }

        private void ChangeResolution(int index)
        {
            _settings.SetResolution(index);
            var value = _resolutionDropdown.options[index].text;
            AnimateValue(_resolutionValue, () => _resolutionValue.text = value);
        }

        private void CycleSpeed() => ChangeSpeed(1);

        private void PreviousSpeed() => ChangeSpeed(-1);

        private void ChangeSpeed(int direction)
        {
            _settings.CycleSequenceSpeed(direction);
            var value = SpeedText();
            AnimateCycleValue(_speedValue, () => _speedValue.text = value, direction);
        }

        private void AnimateCycleValue(TMP_Text label, TweenCallback update, int direction)
        {
            label.DOKill();
            var rect = label.rectTransform;
            var (position, scale) = _valueRestPoses[label];
            rect.anchoredPosition = position;
            rect.localScale = scale;
            var offset = Vector2.right * (direction * rect.rect.width);
            DOTween.Sequence().SetTarget(label).SetUpdate(true)
                .Append(rect.DOAnchorPos(position - offset, 0.1f).SetEase(Ease.InQuad))
                .AppendCallback(() =>
                {
                    update();
                    rect.anchoredPosition = position + offset;
                })
                .Append(rect.DOAnchorPos(position, 0.18f).SetEase(Ease.OutCubic));
        }

        private void AnimateValue(TMP_Text label, TweenCallback update)
        {
            label.DOKill();
            var rect = label.rectTransform;
            var (position, scale) = _valueRestPoses[label];
            DOTween.Sequence().SetTarget(label).SetUpdate(true)
                .Append(rect.DOAnchorPos(position + Vector2.up * 8f, 0.07f).SetEase(Ease.InQuad))
                .Join(rect.DOScale(scale * 0.9f, 0.07f).SetEase(Ease.InQuad))
                .AppendCallback(() =>
                {
                    update();
                    rect.anchoredPosition = position + Vector2.down * 8f;
                    rect.localScale = scale * 0.86f;
                })
                .Append(rect.DOAnchorPos(position, 0.19f).SetEase(Ease.OutCubic))
                .Join(rect.DOScale(scale, 0.19f).SetEase(Ease.OutBack));
        }

        private void StopValueAnimations()
        {
            ResetValue(_displayLabel);
            ResetValue(_resolutionValue);
            ResetValue(_languageValue);
            ResetValue(_speedValue);
        }

        private void ResetValue(TMP_Text label)
        {
            label.DOKill();
            var (position, scale) = _valueRestPoses[label];
            label.rectTransform.anchoredPosition = position;
            label.rectTransform.localScale = scale;
        }

        private string DisplayModeKey() =>
            _settings.DisplayMode switch
            {
                FullScreenMode.ExclusiveFullScreen => "settings_fullscreen",
                FullScreenMode.Windowed => "settings_windowed",
                _ => "settings_borderless"
            };

        private void ChangeLanguage(int index)
        {
            var locale = _locales[index];
            LocalizationSettings.SelectedLocale = locale;
            PlayerPrefs.SetString(LocalePreference, locale.Identifier.Code);
            PlayerPrefs.Save();
        }

        private void UpdateLanguage(Locale locale)
        {
            if (!_wantsOpen) return;

            _languageDropdown.SetValueWithoutNotify(Array.IndexOf(_locales, locale));
            var language = LanguageName(locale);
            AnimateValue(_languageValue, () => _languageValue.text = language);
        }

        private static string LanguageName(Locale locale)
        {
            var culture = locale?.Identifier.CultureInfo;
            var name = culture?.NativeName ?? locale?.LocaleName ?? string.Empty;
            return name.Length == 0
                ? name
                : (culture?.TextInfo.ToUpper(name[0]) ?? char.ToUpperInvariant(name[0])) + name.Substring(1);
        }

        private static string PercentText(int value) => $"{value}%";
        private static string ResolutionText(Vector2Int size) => $"{size.x} × {size.y}";
        private string SpeedText() => "×" + _settings.SequenceSpeed.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
