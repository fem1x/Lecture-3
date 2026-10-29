using DG.Tweening;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class ScreenFlashView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private Ease _inEase = Ease.OutCubic;
        [SerializeField] private Ease _outEase = Ease.OutCubic;
        [SerializeField] private Color _colorDefault = Color.white;
        
        public void DoFlash(Color color=default, float alpha=0.5f, float inTime=0.02f, float outTime=0.55f)
        {
            var flashColor = color == default ? _colorDefault : color;
            flashColor.a = 0;
            _image.color = flashColor;

            DOTween.Sequence()
                .Append(_image.DOFade(alpha, inTime).SetEase(_inEase))
                .Append(_image.DOFade(0, outTime).SetEase(_outEase))
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}