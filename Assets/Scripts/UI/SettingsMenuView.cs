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
using VContainer.Unity;

namespace _Scripts.UI
{
    public sealed class SettingsMenuView : MonoBehaviour, IPointerClickHandler
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
        [SerializeField] private Button _applyButton;
        [SerializeField] private TextRevealView _title;

        [Header("Close Confirmation")]
        [SerializeField] private GameObject _closeConfirmation;
        [SerializeField] private CanvasGroup _confirmationGroup;
        [SerializeField] private RectTransform _confirmationCard;
        [SerializeField, Min(0.01f)] private float _confirmationShowTime = 0.24f;
        [SerializeField, Min(0.01f)] private float _confirmationHideTime = 0.16f;
        [SerializeField, Min(0f)] private float _confirmationOffset = 16f;
        [SerializeField] private Button _stayButton;
        [SerializeField] private Button _discardButton;
        [SerializeField] private Button _applyAndCloseButton;

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
        [SerializeField] private Button _displayButton;
        [SerializeField] private LocalizeStringEvent _displayValue;
        [SerializeField] private TMP_Text _displayLabel;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        [SerializeField] private TMP_Text _resolutionValue;
        [SerializeField] private TMP_Dropdown _fpsLimitDropdown;
        [SerializeField] private TMP_Text _fpsLimitValue;
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private CheckboxFeedbackView _vSyncFeedback;

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
        private Selectable[] _lastTabControls;
        private CancellationTokenSource _transition;
        private Tween _backdropTween;
        private Tween _tabTween;
        private Sequence _entranceTween;
        private Sequence _confirmationTween;
        private Vector2 _confirmationRestPosition;
        private bool _dismissingConfirmation;
        private EntranceItem _closeEntrance;
        private EntranceItem[] _tabEntrances;
        private EntranceItem[][] _pageEntrances;
        private readonly Dictionary<TMP_Text, (Vector2 Position, Vector3 Scale)> _valueRestPoses = new();
        private readonly LocalizedString _unlimitedFpsLabel = new("String Table", "settings_fps_unlimited");
        private GameObject _previousSelection;
        private GameObject _selectionBeforeConfirmation;
        private (int Display, int Resolution, bool VSync, int Fps, int Shake, int Speed, Locale Language) _pending;
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
            _confirmationRestPosition = _confirmationCard.anchoredPosition;
            foreach (var tab in _tabs)
            {
                tab.ShownPosition = ((RectTransform)tab.Page.transform).anchoredPosition;
                tab.BaseColor = tab.Button.image.color;
                tab.AccentView = tab.Accent.GetComponent<SettingsTabAccentView>();
            }

            _closeEntrance = CreateEntranceItem((RectTransform)_closeButton.transform);
            _tabEntrances = _tabs.Select(tab => CreateEntranceItem((RectTransform)tab.Button.transform)).ToArray();
            _pageEntrances = _tabs.Select(tab =>
                tab.Page.transform.Cast<Transform>().Select(child => CreateEntranceItem((RectTransform)child)).ToArray()).ToArray();

            foreach (var label in new[] { _displayLabel, _resolutionValue, _fpsLimitValue, _languageValue, _speedValue })
                _valueRestPoses.Add(label, (label.rectTransform.anchoredPosition, label.rectTransform.localScale));

            _lastTabControls = new Selectable[] { _speedButton, _vSyncToggle, _muteToggle };
            _volumeListeners = new UnityAction<float>[_volumeFields.Length];
            for (var i = 0; i < _volumeListeners.Length; i++)
            {
                var channel = (VolumeChannel)i;
                _volumeListeners[i] = step => ChangeVolume(channel, (int)step * GameSettingsService.VolumeStepPercent);
            }
        }

        private void OnEnable()
        {
            _backdropButton.onClick.AddListener(HandleBackdropClick);
            _closeButton.onClick.AddListener(Hide);
            _applyButton.onClick.AddListener(Apply);
            _stayButton.onClick.AddListener(CancelClose);
            _discardButton.onClick.AddListener(DiscardAndClose);
            _applyAndCloseButton.onClick.AddListener(ApplyAndClose);
            _tabs[0].Button.onClick.AddListener(ShowGameTab);
            _tabs[1].Button.onClick.AddListener(ShowDisplayTab);
            _tabs[2].Button.onClick.AddListener(ShowSoundTab);
            _muteToggle.onValueChanged.AddListener(ToggleMute);
            _vSyncToggle.onValueChanged.AddListener(ToggleVSync);
            _displayButton.onClick.AddListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.AddListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.AddListener(PreviousDisplayMode);
            _resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
            _fpsLimitDropdown.onValueChanged.AddListener(ChangeFpsLimit);
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
            _backdropButton.onClick.RemoveListener(HandleBackdropClick);
            _closeButton.onClick.RemoveListener(Hide);
            _applyButton.onClick.RemoveListener(Apply);
            _stayButton.onClick.RemoveListener(CancelClose);
            _discardButton.onClick.RemoveListener(DiscardAndClose);
            _applyAndCloseButton.onClick.RemoveListener(ApplyAndClose);
            _tabs[0].Button.onClick.RemoveListener(ShowGameTab);
            _tabs[1].Button.onClick.RemoveListener(ShowDisplayTab);
            _tabs[2].Button.onClick.RemoveListener(ShowSoundTab);
            _muteToggle.onValueChanged.RemoveListener(ToggleMute);
            _vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
            _displayButton.onClick.RemoveListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Next Hit Area").GetComponent<Button>().onClick.RemoveListener(CycleDisplayMode);
            _displayButton.transform.Find("(Button) Previous Hit Area").GetComponent<Button>().onClick.RemoveListener(PreviousDisplayMode);
            _resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
            _fpsLimitDropdown.onValueChanged.RemoveListener(ChangeFpsLimit);
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
            _confirmationTween?.Kill();
            _confirmationTween = null;
            _closeConfirmation.SetActive(false);
            _selectionBeforeConfirmation = null;
            _dismissingConfirmation = false;
            _wantsOpen = false;
            if (BlocksGameplayInput)
            {
                _settings.RestoreSound();
                FinishHide();
            }
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Toggle()
        {
            if (_closeConfirmation.activeSelf)
                CancelClose();
            else if (_wantsOpen)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            if (_wantsOpen) return;

            gameObject.SetActive(true);
            if (!isActiveAndEnabled) return;

            _wantsOpen = true;
            if (!BlocksGameplayInput)
            {
                CaptureSelection();
                SelectTab(_activeTab, animate: false);
            }
            _pending = AppliedOptions();
            RefreshControls();
            BlocksGameplayInput = true;
            _backdrop.blocksRaycasts = true;
            SetVisibleAsync(true).Forget();
        }

        public void Hide()
        {
            if (!_wantsOpen || _closeConfirmation.activeSelf) return;

            if (HasPendingChanges)
                ConfirmClose();
            else
                Close();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left &&
                eventData.pointerCurrentRaycast.gameObject == _closeConfirmation)
                CancelClose();
        }

        private void HandleBackdropClick()
        {
            if (_closeConfirmation.activeSelf)
                CancelClose();
            else
                Hide();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void ConfirmClose()
        {
            CloseDropdowns();
            var eventSystem = EventSystem.current;
            _selectionBeforeConfirmation = eventSystem != null ? eventSystem.currentSelectedGameObject : null;

            _panel.interactable = false;
            _confirmationGroup.alpha = 0f;
            _confirmationGroup.interactable = true;
            _confirmationCard.anchoredPosition = _confirmationRestPosition + Vector2.down * _confirmationOffset;
            _closeConfirmation.SetActive(true);
            if (eventSystem != null)
                eventSystem.SetSelectedGameObject(_stayButton.gameObject);

            _confirmationTween = DOTween.Sequence().SetUpdate(true)
                .Join(_confirmationGroup.DOFade(1f, _confirmationShowTime).SetEase(Ease.OutQuad))
                .Join(_confirmationCard.DOAnchorPos(_confirmationRestPosition, _confirmationShowTime).SetEase(Ease.OutCubic))
                .OnComplete(() =>
                {
                    _confirmationTween = null;
                });
        }

        private void CancelClose() => DismissConfirmation(ReturnToSettings);
        private void DiscardAndClose() => DismissConfirmation(Close, closeImmediately: true);
        private void ApplyAndClose() => DismissConfirmation(SaveAndClose, closeImmediately: true);

        private void DismissConfirmation(Action afterDismiss, bool closeImmediately = false)
        {
            if (!_closeConfirmation.activeSelf || _dismissingConfirmation) return;

            _dismissingConfirmation = true;
            _confirmationTween?.Kill();
            _confirmationGroup.interactable = false;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            var exitPosition = _confirmationRestPosition + Vector2.down * (_confirmationOffset * 0.5f);
            _confirmationTween = DOTween.Sequence().SetUpdate(true)
                .Join(_confirmationGroup.DOFade(0f, _confirmationHideTime).SetEase(Ease.InQuad))
                .Join(_confirmationCard.DOAnchorPos(exitPosition, _confirmationHideTime).SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    _confirmationTween = null;
                    _closeConfirmation.SetActive(false);
                    _dismissingConfirmation = false;
                    if (!closeImmediately)
                        afterDismiss();
                    _selectionBeforeConfirmation = null;
                });

            if (closeImmediately)
                afterDismiss();
        }

        private void ReturnToSettings()
        {
            _panel.interactable = true;
            _backdrop.interactable = true;

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(
                    _selectionBeforeConfirmation != null && _selectionBeforeConfirmation.activeInHierarchy
                        ? _selectionBeforeConfirmation
                        : _tabs[_activeTab].Button.gameObject);
        }

        private void SaveAndClose()
        {
            Apply();
            Close();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Close()
        {
            if (!_wantsOpen) return;

            _wantsOpen = false;
            CloseDropdowns();
            _panel.interactable = false;
            _panel.blocksRaycasts = false;
            _backdrop.interactable = false;
            _settings.RestoreSound();
            SetVisibleAsync(false).Forget();
        }

        private void CloseDropdowns()
        {
            if (_resolutionDropdown.IsExpanded)
                _resolutionDropdown.Hide();
            if (_fpsLimitDropdown.IsExpanded)
                _fpsLimitDropdown.Hide();
            if (_languageDropdown.IsExpanded)
                _languageDropdown.Hide();
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
                    _title.Hide().ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token).SuppressCancellationThrow(),
                    _backdropTween.ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, token).SuppressCancellationThrow());
                if (token.IsCancellationRequested) return;
                FinishHide();
                gameObject.SetActive(false);
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
            _entranceTween.Insert(0.12f, _title.Show());
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
            _title.Restore();
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
            var applyNavigation = _applyButton.navigation;
            applyNavigation.selectOnUp = _lastTabControls[index];
            _applyButton.navigation = applyNavigation;
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
            var locale = _pending.Language;
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
            RefreshFpsLimitOptions();
            RefreshFpsLimitAvailability();

            _languageDropdown.ClearOptions();
            _languageDropdown.AddOptions(_locales.Select(LanguageName).ToList());
            _languageDropdown.SetValueWithoutNotify(Array.IndexOf(_locales, locale));
            _languageValue.text = LanguageName(locale);
            _speedValue.text = SpeedText();
            RefreshApplyButton();
        }

        private (int Display, int Resolution, bool VSync, int Fps, int Shake, int Speed, Locale Language) AppliedOptions() =>
            (_settings.DisplayIndex, _settings.ResolutionIndex, _settings.VSync, _settings.FpsLimitIndex,
                _settings.ShakeSteps, _settings.SpeedIndex, LocalizationSettings.SelectedLocale);

        private bool HasPendingChanges => _pending != AppliedOptions() || _settings.HasSoundChanges;

        private void RefreshApplyButton()
        {
            var hasChanges = HasPendingChanges;
            _applyButton.interactable = hasChanges;
            foreach (var control in _lastTabControls)
            {
                var navigation = control.navigation;
                navigation.selectOnDown = hasChanges ? _applyButton : null;
                control.navigation = navigation;
            }

            if (!hasChanges && EventSystem.current != null &&
                EventSystem.current.currentSelectedGameObject == _applyButton.gameObject)
                EventSystem.current.SetSelectedGameObject(_lastTabControls[_activeTab].gameObject);
        }

        private void Apply()
        {
            if (!_wantsOpen || !HasPendingChanges) return;

            _settings.SetDisplay(_pending.Display, _pending.Resolution);
            _settings.SetVSync(_pending.VSync);
            _settings.SetFpsLimit(_pending.Fps);
            _settings.SetShakeSteps(_pending.Shake);
            _settings.SetSequenceSpeed(_pending.Speed);
            if (_settings.HasSoundChanges)
                _settings.SaveSound();

            var locale = _pending.Language;
            if (locale != LocalizationSettings.SelectedLocale)
            {
                LocalizationSettings.SelectedLocale = locale;
                PlayerPrefs.SetString(LocalePreference, locale.Identifier.Code);
            }

            PlayerPrefs.Save();
            RefreshApplyButton();
        }

        private void ChangeVolume(VolumeChannel channel, int value)
        {
            _settings.PreviewVolume(channel, value);
            _volumeFields[(int)channel].Value.text = PercentText(value);
            RefreshApplyButton();
        }

        private void ChangeShake(float steps)
        {
            _pending.Shake = (int)steps;
            _shakeField.Value.text = PercentText(_pending.Shake * GameSettingsService.ShakeStepPercent);
            RefreshApplyButton();
        }

        private void ToggleMute(bool enabled)
        {
            _settings.PreviewMuteWhenUnfocused(enabled);
            RefreshApplyButton();
            _sfxPlayer.Play("ButtonClick");
        }

        private void ToggleVSync(bool enabled)
        {
            _pending.VSync = enabled;
            RefreshFpsLimitAvailability();
            RefreshApplyButton();
            _sfxPlayer.Play("ButtonClick");
        }

        private void RefreshFpsLimitAvailability()
        {
            var available = GameSettingsService.IsFpsLimitAvailable(_pending.VSync);
            _fpsLimitDropdown.interactable = available;
            var navigation = _resolutionDropdown.navigation;
            navigation.selectOnDown = available ? _fpsLimitDropdown : _vSyncToggle;
            _resolutionDropdown.navigation = navigation;
            navigation = _vSyncToggle.navigation;
            navigation.selectOnUp = available ? _fpsLimitDropdown : _resolutionDropdown;
            _vSyncToggle.navigation = navigation;
        }

        private void RefreshFpsLimitOptions()
        {
            var unlimited = _unlimitedFpsLabel.GetLocalizedString();
            var options = _settings.FpsLimits.Select(fps => fps > 0 ? $"{fps} FPS" : unlimited).ToList();
            var index = _pending.Fps;
            _fpsLimitDropdown.ClearOptions();
            _fpsLimitDropdown.AddOptions(options);
            _fpsLimitDropdown.SetValueWithoutNotify(index);
            _fpsLimitValue.text = options[index];
        }

        private void CycleDisplayMode() => ChangeDisplayMode(1);

        private void PreviousDisplayMode() => ChangeDisplayMode(-1);

        private void ChangeDisplayMode(int direction)
        {
            var count = GameSettingsService.ScreenModes.Count;
            _pending.Display = (_pending.Display + direction + count) % count;
            RefreshApplyButton();
            var key = DisplayModeKey();
            AnimateCycleValue(_displayLabel, () => _displayValue.SetEntry(key), direction);
        }

        private void ChangeResolution(int index)
        {
            _pending.Resolution = index;
            RefreshApplyButton();
            var value = _resolutionDropdown.options[index].text;
            AnimateValue(_resolutionValue, () => _resolutionValue.text = value);
        }

        private void ChangeFpsLimit(int index)
        {
            _pending.Fps = index;
            RefreshApplyButton();
            var value = _fpsLimitDropdown.options[index].text;
            AnimateValue(_fpsLimitValue, () => _fpsLimitValue.text = value);
        }

        private void CycleSpeed() => ChangeSpeed(1);

        private void PreviousSpeed() => ChangeSpeed(-1);

        private void ChangeSpeed(int direction)
        {
            var count = GameSettingsService.SequenceSpeeds.Count;
            _pending.Speed = (_pending.Speed + direction + count) % count;
            RefreshApplyButton();
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
            ResetValue(_fpsLimitValue);
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
            GameSettingsService.ScreenModes[_pending.Display] switch
            {
                FullScreenMode.ExclusiveFullScreen => "settings_fullscreen",
                FullScreenMode.Windowed => "settings_windowed",
                _ => "settings_borderless"
            };

        private void ChangeLanguage(int index)
        {
            _pending.Language = _locales[index];
            var language = LanguageName(_pending.Language);
            AnimateValue(_languageValue, () => _languageValue.text = language);
            RefreshApplyButton();
        }

        private void UpdateLanguage(Locale _)
        {
            if (!_wantsOpen) return;

            ResetValue(_fpsLimitValue);
            RefreshFpsLimitOptions();
            RefreshApplyButton();
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
        private string SpeedText() =>
            "×" + GameSettingsService.SequenceSpeeds[_pending.Speed].ToString("0.##", CultureInfo.InvariantCulture);
    }

    public sealed class SettingsMenuInput : ITickable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly SettingsMenuView _view;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public SettingsMenuInput(SettingsMenuView view)
        {
            _view = view;
        }

        public void Tick()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;

            _view.Toggle();
        }
    }

}
