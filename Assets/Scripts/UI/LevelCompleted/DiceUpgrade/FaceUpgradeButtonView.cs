using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Scripts.UI.Workshop
{
    public class FaceUpgradeButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Button _button;
        [SerializeField] private Image _faceImage;
        [SerializeField] private TMP_Text _bonusText;
        [SerializeField] private Image _background;

        [SerializeField] private Color _normalColor;
        [SerializeField] private Color _upgradedColor;

        private int _faceIndex;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int> OnFaceHovered;
        public event Action<int> OnFaceUnhovered;
        public event Action<int> OnFaceClicked;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private void Awake() => _button.onClick.AddListener(() => OnFaceClicked?.Invoke(_faceIndex));
        private void OnDestroy() => _button.onClick.RemoveAllListeners();

        public void OnPointerEnter(PointerEventData eventData) => OnFaceHovered?.Invoke(_faceIndex);

        public void OnPointerExit(PointerEventData eventData) => OnFaceUnhovered?.Invoke(_faceIndex);

        public void Init(int faceIndex) => _faceIndex = faceIndex;

        public void Refresh(Sprite faceSprite, int bonus, bool canAfford)
        {
            _faceImage.sprite = faceSprite;
            _bonusText.text = bonus > 0 ? $"+{bonus}" : "+0";
            _button.interactable = canAfford;
            _background.color = bonus > 0 ? _upgradedColor : _normalColor;
        }
    }
}