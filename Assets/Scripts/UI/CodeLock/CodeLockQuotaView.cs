using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.CodeLock
{
    public class CodeLockQuotaView : MonoBehaviour
    {
        [Header("Quota Overlay")]
        [SerializeField] private Image _overlay;
        [SerializeField] private Image _icon;
        [SerializeField] private RectTransform _iconRect;
        
        [Header("Fade Animation")]
        [SerializeField] private float _fadeTime = 0.5f;

        public void Show()
        {
            _icon.color = new Color(_icon.color.r, _icon.color.g, _icon.color.b, 0);
            _overlay.DOFade(1f,  _fadeTime).SetEase(Ease.OutCubic)
                .OnComplete(ShowIcon);
        }

        public void Hide()
        {
            _iconRect.DoScalePop();
            
            _icon.DOFade(0f, _fadeTime).SetEase(Ease.OutCubic);
            _overlay.DOFade(0, _fadeTime).SetEase(Ease.OutCubic);
        }

        private void ShowIcon()
        {
            _icon.color =  new Color(_icon.color.r, _icon.color.g, _icon.color.b, 1);
            _iconRect.DoScalePop(scaleMult: 1.5f ,outTime: 0.7f);
        }
    }
}