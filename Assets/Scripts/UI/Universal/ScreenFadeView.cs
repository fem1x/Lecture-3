using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class ScreenFadeView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private float _timeIn = 0.2f;
        [SerializeField] private float _timeOut = 1f;
        [SerializeField] private Ease _easeIn = Ease.OutQuad;
        [SerializeField] private Ease _easeOut = Ease.OutQuad;

        internal float FadeInDuration => _timeIn;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _cg.alpha = 1f;

        public async UniTask FadeInAsync(float? time = null, Ease? ease = null)
        {
            _cg.DOKill();
            
            _cg.gameObject.SetActive(true);
            _cg.blocksRaycasts = true;
            
            var appliedEase =  ease ?? _easeIn;
            await _cg.DOFade(1f, time ?? _timeIn)
                .SetEase(appliedEase)
                .SetUpdate(true)
                .SetLink(gameObject);
        }

        public async UniTask FadeOutAsync(float? time = null, Ease? ease = null)
        {
            await CreateFadeOutTween(time, ease);
        }

        internal Tween CreateFadeOutTween(float? time = null, Ease? ease = null)
        {
            _cg.DOKill();

            return _cg.DOFade(0f, time ?? _timeOut)
                .SetEase(ease ?? _easeOut)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    _cg.blocksRaycasts = false;
                    _cg.gameObject.SetActive(false);
                });
        }
    }
}
