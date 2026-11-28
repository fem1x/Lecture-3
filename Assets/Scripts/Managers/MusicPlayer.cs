using DG.Tweening;
using UnityEngine;

namespace _Scripts.Managers
{
    public sealed class MusicPlayer : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private AudioSource _menuSource;
        [SerializeField] private AudioSource _gameplaySource;
        private const double MenuScheduleLead = 0.1;

        private float _menuVolume;
        private float _gameplayVolume;
        private double _menuStartTime;
        private Tween _menuFade;
        private Tween _gameplayFade;

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake()
        {
            _menuVolume = _menuSource.volume;
            _gameplayVolume = _gameplaySource.volume;
        }

        internal double ScheduleMenu()
        {
            _menuFade?.Kill();
            _menuSource.Stop();
            _menuSource.volume = _menuVolume;
            _menuStartTime = AudioSettings.dspTime + MenuScheduleLead;
            _menuSource.PlayScheduled(_menuStartTime);
            return _menuStartTime;
        }

        internal void FadeOutMenu(float duration)
        {
            if (AudioSettings.dspTime < _menuStartTime)
            {
                _menuSource.Stop();
                return;
            }

            if (_menuFade == null && _menuSource.isPlaying)
                _menuFade = FadeOut(_menuSource, duration).OnKill(() => _menuFade = null);
        }

        internal void PlayGameplay()
        {
            if (_gameplayFade == null && _gameplaySource.isPlaying) return;

            _gameplayFade?.Kill();
            _gameplaySource.Stop();
            _gameplaySource.volume = _gameplayVolume;
            _gameplaySource.Play();
        }

        internal void FadeOutGameplay(float duration)
        {
            if (_gameplayFade == null && _gameplaySource.isPlaying)
                _gameplayFade = FadeOut(_gameplaySource, duration).OnKill(() => _gameplayFade = null);
        }

        private Tween FadeOut(AudioSource source, float duration) => source.DOFade(0f, duration)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetLink(gameObject)
            .OnComplete(source.Stop);
    }
}
