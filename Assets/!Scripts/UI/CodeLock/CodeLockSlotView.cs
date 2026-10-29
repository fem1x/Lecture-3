using System.Collections.Generic;
using _Scripts.Configs;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.CodeLock
{
    public class CodeLockSlotView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        [field: SerializeField] public Image DiceImage { get; private set; }
        [SerializeField] private List<Sprite> _diceSprites;
        
        [Header("Active / Inactive")]
        [SerializeField] private Image _activeOverlayImage;

        [Header("Colors")] 
        [SerializeField] private Color _activeColor;
        [SerializeField] private Color _inactiveColor;
        
        [Header("Flash animation")]
        [SerializeField] private float _flashAlpha;
        [SerializeField] private float _afterFlashAlpha;
        [SerializeField] private float _flashInTime;
        [SerializeField] private float _flashOutTime;
        [SerializeField] private Ease _flashInEase;
        [SerializeField] private Ease _flashOutEase;
        
        private ColorsConfig _colorsConfig;

        [Inject]
        public void Construct(ColorsConfig colorsConfig)
        {
            _colorsConfig = colorsConfig;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => RemoveValue();
        
        public void InsertValue(int value)
        {
            DiceImage.sprite = _diceSprites[value];
            DiceImage.gameObject.SetActive(true);
            
            DiceImage.transform.DoScalePop();
        }

        public void RemoveValue()
        {
            DiceImage.sprite = null;
            DiceImage.gameObject.SetActive(false);
        }

        public void SetActiveState(bool isActive)
        {
            _activeOverlayImage.color = isActive ? _activeColor : _inactiveColor;
        }

        public void SetCorrectState(LockBreakResult result)
        {
            var color = result == LockBreakResult.Success ? _colorsConfig.Success : _colorsConfig.Failure;
            DoFlashTween(color);
        }

        private void DoFlashTween(Color flashColor)
        {
            flashColor.a = 0;
            _activeOverlayImage.color = flashColor;

            DOTween.Sequence()
                .Append(_activeOverlayImage.DOFade(_flashAlpha, _flashInTime).SetEase(_flashInEase))
                .Append(_activeOverlayImage.DOFade(_afterFlashAlpha, _flashOutTime).SetEase(_flashOutEase));
        }
    }
}