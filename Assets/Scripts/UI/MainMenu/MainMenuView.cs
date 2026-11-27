using _Scripts.Dices;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.MainMenu
{
    public sealed class MainMenuView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private CanvasGroup _actions;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private StaggeredRevealView _entrance;
        [SerializeField] private MenuDiceShowcase _showcase;

        [Header("Quit dialog")]
        [SerializeField] private CanvasGroup _quitDialog;
        [SerializeField] private RectTransform _quitCard;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _confirmExitButton;
        [SerializeField] private TMP_Text _quitTitle;

        private GameStateManager _gameStateManager;
        private Bootstrap _bootstrap;
        private SettingsMenuView _settings;
        private ScreenFadeView _screenFade;
        private (LocalizedString Key, LocalizedString.ChangeHandler Listener)[] _labels;
        private Sequence _dialogTween;
        private Vector2 _quitCardPosition;
        private bool _starting;
        private bool _settingsActive;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(GameStateManager gameStateManager, Bootstrap bootstrap,
            SettingsMenuView settings, ScreenFadeView screenFade)
        {
            _gameStateManager = gameStateManager;
            _bootstrap = bootstrap;
            _settings = settings;
            _screenFade = screenFade;
        }

        private void Awake()
        {
            _quitCardPosition = _quitCard.anchoredPosition;
            _labels = new[]
            {
                Label("main_menu_play", _playButton.GetComponentInChildren<TMP_Text>(true)),
                Label("pause_settings", _settingsButton.GetComponentInChildren<TMP_Text>(true)),
                Label("main_menu_quit", _exitButton.GetComponentInChildren<TMP_Text>(true)),
                Label("main_menu_quit_title", _quitTitle),
                Label("main_menu_back", _backButton.GetComponentInChildren<TMP_Text>(true)),
                Label("main_menu_quit", _confirmExitButton.GetComponentInChildren<TMP_Text>(true))
            };
        }

        private void Start()
        {
            _gameStateManager.StateChanged += OnStateChanged;
            _playButton.onClick.AddListener(Play);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(OpenQuit);
            _backButton.onClick.AddListener(CloseQuit);
            _confirmExitButton.onClick.AddListener(Quit);
            OnStateChanged(_gameStateManager.CurrentState);
        }

        private void OnEnable()
        {
            foreach (var (key, listener) in _labels)
                key.StringChanged += listener;
        }

        private void OnDisable()
        {
            foreach (var (key, listener) in _labels)
                key.StringChanged -= listener;
            _dialogTween?.Kill();
        }

        private void OnDestroy()
        {
            if (_gameStateManager != null)
                _gameStateManager.StateChanged -= OnStateChanged;
            _playButton.onClick.RemoveListener(Play);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _exitButton.onClick.RemoveListener(OpenQuit);
            _backButton.onClick.RemoveListener(CloseQuit);
            _confirmExitButton.onClick.RemoveListener(Quit);
        }

        private void Update()
        {
            if (_settingsActive && !SettingsMenuView.BlocksGameplayInput)
            {
                _settingsActive = false;
                SetActionsEnabled(true);
                Select(_settingsButton);
            }

            if (_quitDialog.gameObject.activeSelf && _quitDialog.interactable &&
                Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseQuit();

            _showcase.SetPlayHovered(_actions.interactable && Mouse.current != null &&
                RectTransformUtility.RectangleContainsScreenPoint((RectTransform)_playButton.transform,
                    Mouse.current.position.ReadValue()));
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void OnStateChanged(GameState state)
        {
            var visible = state == GameState.MainMenu;
            if (!visible)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            _starting = false;
            _settingsActive = false;
            _dialogTween?.Kill();
            _quitDialog.gameObject.SetActive(false);
            _cg.interactable = true;
            _cg.blocksRaycasts = true;
            SetActionsEnabled(true);
            _showcase.Show();
            _entrance.Show();
            Select(_playButton);
        }

        private void Play()
        {
            if (!_actions.interactable || _starting) return;
            _starting = true;
            SetActionsEnabled(false);
            Select(null);
            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();
            var game = _bootstrap;
            var fade = _screenFade;
            var roll = _showcase.PlayRoll();
            if (await UniTask.Delay(260, ignoreTimeScale: true, cancellationToken: token).SuppressCancellationThrow())
                return;
            if (!isActiveAndEnabled) return;

            await fade.FadeInAsync(0.24f);
            if (token.IsCancellationRequested) return;
            if (isActiveAndEnabled && _gameStateManager.CurrentState == GameState.MainMenu)
            {
                roll.Kill();
                game.StartGame();
            }
            await fade.FadeOutAsync(0.48f);
        }

        private void OpenSettings()
        {
            if (!_actions.interactable) return;
            _settingsActive = true;
            _settings.Show();
            SetActionsEnabled(false);
        }

        private void OpenQuit()
        {
            if (!_actions.interactable) return;
            SetActionsEnabled(false);
            _quitDialog.gameObject.SetActive(true);
            _quitDialog.alpha = 0f;
            _quitDialog.interactable = false;
            _quitCard.anchoredPosition = _quitCardPosition + Vector2.down * 16f;
            Select(null);

            _dialogTween?.Kill();
            _dialogTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            _dialogTween.Join(_quitDialog.DOFade(1f, 0.24f).SetEase(Ease.OutQuad).SetUpdate(true).SetLink(gameObject));
            _dialogTween.Join(_quitCard.DOAnchorPos(_quitCardPosition, 0.24f).SetEase(Ease.OutCubic).SetUpdate(true).SetLink(gameObject));
            _dialogTween.OnComplete(() =>
            {
                _quitDialog.interactable = true;
                Select(_backButton);
            });
        }

        private void CloseQuit()
        {
            if (!_quitDialog.interactable) return;
            _quitDialog.interactable = false;
            Select(null);

            _dialogTween?.Kill();
            _dialogTween = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            _dialogTween.Join(_quitDialog.DOFade(0f, 0.16f).SetEase(Ease.InQuad).SetUpdate(true).SetLink(gameObject));
            _dialogTween.Join(_quitCard.DOAnchorPos(_quitCardPosition + Vector2.down * 8f, 0.16f)
                .SetEase(Ease.InQuad).SetUpdate(true).SetLink(gameObject));
            _dialogTween.OnComplete(() =>
            {
                _quitDialog.gameObject.SetActive(false);
                SetActionsEnabled(true);
                Select(_exitButton);
            });
        }

        private void Quit()
        {
            if (_quitDialog.interactable)
                Application.Quit();
        }

        private void SetActionsEnabled(bool enabled)
        {
            _actions.interactable = enabled;
            _actions.blocksRaycasts = enabled;
            _showcase.SetInteractionEnabled(enabled);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(button != null ? button.gameObject : null);
        }

        private static (LocalizedString Key, LocalizedString.ChangeHandler Listener) Label(string key, TMP_Text text)
        {
            var fallback = text.text;
            return (new LocalizedString("String Table", key), value =>
                text.text = string.IsNullOrEmpty(value) || value.StartsWith("No translation found") ? fallback : value);
        }
    }
}
