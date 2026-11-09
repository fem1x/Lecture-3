using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.UI
{
    public class ScreenFadeView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private float _defaultDuration = 0.5f;
        [SerializeField] private Ease _easeIn = Ease.OutQuad;
        [SerializeField] private Ease _easeOut = Ease.OutQuad;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _cg.alpha = 1f;

        private void Start() => FadeOutAsync().Forget();

        public async UniTask FadeInAsync(float? duration = null)
        {
            _cg.gameObject.SetActive(true);
            _cg.blocksRaycasts = true;
            
            await _cg.DOFade(1f, duration ?? _defaultDuration)
                .SetEase(_easeIn)
                .SetLink(gameObject);
        }

        public async UniTask FadeOutAsync(float? duration = null)
        {
            await _cg.DOFade(0f, duration ?? _defaultDuration)
                .SetEase(_easeOut)
                .SetLink(gameObject);
            
            if (this == null || _cg == null) return;
            _cg.blocksRaycasts = false;
            _cg.gameObject.SetActive(false);
        }
    }
}