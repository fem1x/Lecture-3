using System.Threading.Tasks;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class MapLevelPointerView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _holderRect;
        [SerializeField] private RectTransform _pointerRect;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public async UniTaskVoid MoveToCellAsync(RectTransform cellRect)
        {
            await UniTask.WhenAll(
                _pointerRect.DoScaleOut().ToUniTask(),
                _image.DoFadeOut(time: 0.4f, ease: Ease.OutFlash).ToUniTask());
            
            if (cellRect == null) return;
            _holderRect.SetParent(cellRect, worldPositionStays: false); 
            _pointerRect.localScale = Vector3.one;
            
            await UniTask.WhenAll(
                _pointerRect.DoScaleIn(ease: Ease.OutBack).ToUniTask(),
                _image.DoFadeIn().ToUniTask());
        }

        public void Hide()
        {
            _holderRect.DoScaleOut().OnComplete(() => _image.SetVisible(false));
        }

        public void Show()
        {
            _image.SetVisible(true);
            _holderRect.DoScaleIn();
        }
    }
}