using System.Threading;
using _Scripts.Dices;
using _Scripts.Managers;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI
{
    public sealed class PauseMenuView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private SlidingPanelView _slide;
        [SerializeField] private TextRevealView _title;
        [SerializeField] private StaggeredRevealView _buttonsReveal;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField, Min(0f)] private float _pauseTime = 0.3f;
        [SerializeField] private Ease _pauseEase = Ease.OutCubic;
        [SerializeField, Min(0f)] private float _resumeTime = 0.3f;
        [SerializeField] private Ease _resumeEase = Ease.OutCubic;

        [SerializeField] private PauseSettingsTransition _settingsTransition;

        private GameStateManager _gameState;
        private SettingsMenuView _settings;
        private TransitionService _transitionService;
        private CancellationTokenSource _animation;
        private Tween _timeTween;
        private Sequence _entranceTween;
        private bool _paused;
        private bool _settingsActive;
        private bool _settingsMorphing;
        private bool _leaving;
        private GameObject _previousSelection;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Inject]
        public void Construct(GameStateManager gameState, SettingsMenuView settings, TransitionService transitionService)
        {
            _gameState = gameState;
            _settings = settings;
            _transitionService = transitionService;
        }

        private void Start()
        {
            _resumeButton.onClick.AddListener(Resume);
            _settingsButton.onClick.AddListener(OpenSettings);
            _mainMenuButton.onClick.AddListener(ReturnToMenu);
            _settings.ClosingFromPause += ReturnFromSettings;
            _gameState.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            _resumeButton.onClick.RemoveListener(Resume);
            _settingsButton.onClick.RemoveListener(OpenSettings);
            _mainMenuButton.onClick.RemoveListener(ReturnToMenu);
            _settings.ClosingFromPause -= ReturnFromSettings;
            _gameState.StateChanged -= OnStateChanged;
            ResetPause();
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public void HandleEscape()
        {
            if (_leaving) return;

            if (_settingsActive)
            {
                if (_settings.IsOpen && !_settingsMorphing) _settings.Toggle();
                return;
            }

            if (_settings.IsOpen)
            {
                _settings.Toggle();
                return;
            }

            if (_gameState.CurrentState != GameState.Game) return;

            if (_paused)
                Resume();
            else
                Open();
        }

        private void Open()
        {
            gameObject.SetActive(true);
            _paused = true;
            SettingsMenuView.PauseBlocksGameplayInput = true;
            _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            _settings.ShowBackdrop();
            AnimateTime(0f, _pauseTime, _pauseEase);
            ShowPanel();
        }

        private void ShowPanel()
        {
            var token = RestartAnimation();
            _slide.ShowAsync(token).SuppressCancellationThrow().Forget();
            PlayEntrance();
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(_resumeButton.gameObject);
        }

        private void Resume()
        {
            if (!_paused || _settingsActive || _leaving) return;

            _paused = false;
            _settings.HideBackdrop();
            AnimateTime(1f, _resumeTime, _resumeEase);
            FinishResumeAsync(RestartAnimation()).Forget();
        }

        private async UniTaskVoid FinishResumeAsync(CancellationToken token)
        {
            PlayTitleHide();
            await _slide.HideAsync(token).SuppressCancellationThrow();
            if (token.IsCancellationRequested) return;

            ResetEntrance();
            SettingsMenuView.PauseBlocksGameplayInput = false;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(
                    _previousSelection != null && _previousSelection.activeInHierarchy ? _previousSelection : null);
            _previousSelection = null;
            gameObject.SetActive(false);
        }

        private void OpenSettings()
        {
            if (!_paused || _settingsActive || _leaving) return;

            _settingsActive = true;
            MorphSettingsAsync(opening: true).Forget();
        }

        private void ReturnFromSettings()
        {
            if (!_paused || _leaving) return;
            MorphSettingsAsync(opening: false).Forget();
        }

        private async UniTaskVoid MorphSettingsAsync(bool opening)
        {
            _settingsMorphing = true;
            var token = RestartAnimation(resetEntrance: !opening);
            _entranceTween?.Kill();
            await _settingsTransition.PlayAsync(opening, _slide, _title, _settings, _settingsButton, OnReveal, token)
                .SuppressCancellationThrow();
            if (token.IsCancellationRequested) return;

            _settingsMorphing = false;
            if (opening)
                ResetEntrance();

            void OnReveal()
            {
                _settingsMorphing = false;
                if (!opening)
                {
                    _settingsActive = false;
                    _buttonsReveal.Show(0f);
                }
            }
        }

        private void ReturnToMenu()
        {
            if (!_paused || _settingsActive || _leaving) return;

            _leaving = true;
            _timeTween?.Kill();
            Time.timeScale = 0f;
            _transitionService.TransitionToStateAsync(GameState.MainMenu).Forget();
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Game)
                ResetPause();
        }

        private void ResetPause()
        {
            if (!_paused && !_leaving && !SettingsMenuView.PauseBlocksGameplayInput &&
                (_timeTween == null || !_timeTween.IsActive())) return;

            _animation?.Cancel();
            _animation?.Dispose();
            _animation = null;
            ResetEntrance();
            _timeTween?.Kill();
            Time.timeScale = 1f;
            _slide.HideInstant();
            if (_settings)
            {
                _settings.FinishClosingFromPause();
                _settings.HideBackdrop();
            }
            SettingsMenuView.PauseBlocksGameplayInput = false;
            _paused = false;
            _settingsActive = false;
            _settingsMorphing = false;
            _leaving = false;
            _previousSelection = null;
            gameObject.SetActive(false);
        }

        private CancellationToken RestartAnimation(bool resetEntrance = true)
        {
            _animation?.Cancel();
            _animation?.Dispose();
            if (resetEntrance)
                ResetEntrance();
            _animation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            return _animation.Token;
        }

        private void PlayEntrance()
        {
            _entranceTween = DOTween.Sequence().SetUpdate(true);
            _entranceTween.Insert(0.12f, _title.Show());
            _buttonsReveal.Show();
        }

        private void PlayTitleHide()
        {
            _entranceTween = DOTween.Sequence().SetUpdate(true).Append(_title.Hide());
        }

        private void ResetEntrance()
        {
            _entranceTween?.Kill();
            _entranceTween = null;
            _title.Restore();
            _buttonsReveal.Restore();
        }

        private void AnimateTime(float target, float duration, Ease ease)
        {
            _timeTween?.Kill();
            _timeTween = DOTween.To(() => Time.timeScale, value => Time.timeScale = value, target, duration)
                .SetEase(ease).SetUpdate(true);
        }
    }

    public sealed class PauseMenuInput : ITickable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly PauseMenuView _pause;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public PauseMenuInput(PauseMenuView pause) => _pause = pause;

        public void Tick()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                _pause.HandleEscape();
        }
    }
}
