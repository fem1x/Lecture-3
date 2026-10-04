using System.Collections.Generic;
using _Scripts.Dices;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DiceNetView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Face Images")]
        [SerializeField] private Image _upImage;
        [SerializeField] private Image _downImage;
        [SerializeField] private Image _rightImage;
        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _forwardImage;
        [SerializeField] private Image _backImage;
        [Space]
        [SerializeField] private Sprite[] _faceSprites;
        [Space]
        [SerializeField] private DurabilityView _durabilityView;
        
        private Image[] _faceImages;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _faceImages = new[]
            {
                _upImage,
                _downImage,
                _rightImage,
                _leftImage,
                _forwardImage,
                _backImage
            };
        }
        
        public void UpdateView(DiceFace[] faces, int currentDurability, int maxDurability)
        {
            for (int i = 0; i < faces.Length; i++)
            {
                _faceImages[i].sprite = _faceSprites[faces[i].CurrentValue];
            }
            
            _durabilityView.SetDurability(currentDurability, maxDurability);
        }
    }
}