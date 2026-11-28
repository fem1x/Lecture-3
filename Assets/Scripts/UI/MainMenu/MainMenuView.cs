using _Scripts.Dices;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

        [Header("Fade transition")] 
        [SerializeField] private float _inTime = 0.5f;
        [SerializeField] private Ease _inEase = Ease.InOutCubic;
        [Space]
        [SerializeField] private float _outTime = 2f;
        [SerializeField] private Ease _outEase = Ease.OutQuad;
        
        
        private GameStateManager _gameStateManager;
        private Bootstrap _bootstrap;
        private SettingsMenuView _settings;
        private ScreenFadeView _screenFade;
        private ISfxPlayer _sfx;
        private MusicPlayer _musicPlayer;
        private Sequence _dialogTween;
        private Sequence _menuEntrance;
        private Tween _menuScreenFade;
        private double? _menuStartTime;
        private Vector2 _quitCardPosition;
        private bool _starting;
        private bool _settingsActive;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(GameStateManager gameStateManager, Bootstrap bootstrap,
            SettingsMenuView settings, ScreenFadeView screenFade, ISfxPlayer sfx, MusicPlayer musicPlayer)
        {
            _gameStateManager = gameStateManager;
            _bootstrap = bootstrap;
            _settings = settings;
            _screenFade = screenFade;
            _sfx = sfx;
            _musicPlayer = musicPlayer;
            _gameStateManager.StateChanged += OnStateChanged;
            _playButton.onClick.AddListener(Play);
            _settingsButton.onClick.AddListener(OpenSettings);
            _exitButton.onClick.AddListener(OpenQuit);
            _backButton.onClick.AddListener(CloseQuit);
            _confirmExitButton.onClick.AddListener(Quit);
        }

        private void Awake() => _quitCardPosition = _quitCard.anchoredPosition;

        private void OnDisable()
        {
            _dialogTween?.Kill();
            _menuEntrance?.Kill();
            _menuScreenFade?.Kill();
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
            UpdateEntrance();

            if (_settingsActive && !SettingsMenuView.BlocksGameplayInput)
            {
                _settingsActive = false;
                SetActionsEnabled(true);
                Select(_settingsButton);
            }

            if (_quitDialog.gameObject.activeSelf && _quitDialog.interactable &&
                Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                CloseQuit();

            if (!_actions.interactable || Mouse.current == null)
            {
                _showcase.SetButtonHover(false, false);
                return;
            }

            var pointer = Mouse.current.position.ReadValue();
            _showcase.SetButtonHover(IsHovered(_playButton, pointer), IsHovered(_exitButton, pointer));
        }

        private static bool IsHovered(Button button, Vector2 pointer) =>
            button.isActiveAndEnabled && button.IsInteractable() &&
            RectTransformUtility.RectangleContainsScreenPoint((RectTransform)button.transform, pointer);

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void OnStateChanged(GameState state)
        {
            var visible = state == GameState.MainMenu;
            if (!visible)
            {
                if (gameObject.activeSelf && !_starting)
                {
                    _musicPlayer.FadeOutMenu(_inTime);
                    if (_menuScreenFade != null)
                        _screenFade.FadeOutAsync().Forget();
                }

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
            _musicPlayer.FadeOutGameplay(_screenFade.FadeInDuration);
            PrepareEntrance();
            Select(_playButton);
        }

        private void PrepareEntrance()
        {
            _menuEntrance?.Kill();
            _menuStartTime = null;
            _menuEntrance = DOTween.Sequence().SetUpdate(true).SetLink(gameObject)
                .SetAutoKill(false).Pause().OnKill(() => _menuEntrance = null);
            _menuEntrance.Join(_showcase.Show());
            _menuEntrance.Join(_entrance.Show(() => _sfx.Play("ElementAppear")));
            _menuScreenFade = _screenFade.CreateFadeOutTween().SetAutoKill(false).Pause()
                .OnKill(() => _menuScreenFade = null);
        }

        private void UpdateEntrance()
        {
            if (_menuEntrance == null && _menuScreenFade == null) return;

            // Schedule on the first update so scene initialization cannot consume the music intro.
            _menuStartTime ??= _starting ? AudioSettings.dspTime : _musicPlayer.ScheduleMenu();
            var elapsed = (float)(AudioSettings.dspTime - _menuStartTime.Value);
            if (elapsed <= 0f) return;

            SeekEntrance(_menuEntrance, elapsed);
            SeekEntrance(_menuScreenFade, elapsed);
        }

        private static void SeekEntrance(Tween tween, float elapsed)
        {
            if (tween == null) return;

            tween.GotoWithCallbacks(elapsed);
            if (tween.IsComplete())
                tween.Kill();
        }

        private void Play()
        {
            if (!_actions.interactable || _starting) return;
            _starting = true;
            _musicPlayer.FadeOutMenu(_inTime);
            SetActionsEnabled(false);
            Select(null);
            PlayAsync().Forget();
        }

        private async UniTaskVoid PlayAsync()
        {
            var token = this.GetCancellationTokenOnDestroy();
            var game = _bootstrap;
            var fade = _screenFade;
            _showcase.PlayRoll();
            await fade.FadeInAsync(time: _inTime, ease: _inEase);
            if (token.IsCancellationRequested) return;
            if (isActiveAndEnabled && _gameStateManager.CurrentState == GameState.MainMenu)
            {
                game.StartGame();
            }
            await fade.FadeOutAsync(time: _outTime, ease: _outEase);
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

    }
}
