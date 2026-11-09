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
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake() => _cg.alpha = 1f;

        private void Start() => FadeOutAsync(time: 3f).Forget();

        public async UniTask FadeInAsync(float? time = null)
        {
            _cg.DOKill();
            
            _cg.gameObject.SetActive(true);
            _cg.blocksRaycasts = true;
            
            await _cg.DOFade(1f, time ?? _timeIn)
                .SetEase(_easeIn)
                .SetLink(gameObject);
        }

        public async UniTask FadeOutAsync(float? time = null)
        {
            _cg.DOKill();
            
            await _cg.DOFade(0f, time ?? _timeOut)
                .SetEase(_easeOut)
                .SetLink(gameObject);
            
            if (this == null || _cg == null) return;
            
            if (Mathf.Approximately(_cg.alpha, 0f))
            {
                _cg.blocksRaycasts = false;
                _cg.gameObject.SetActive(false);
            }
        }
    }
}