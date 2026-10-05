using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
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
        private IReadOnlyDiceData _data;
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

        public void Bind(IReadOnlyDiceData data)
        {
            _data = data;
            _data.OnDataChanged += Refresh;
            Refresh();
        }
        
        private void Refresh()
        {
            for (int i = 0; i < _faceImages.Length; i++)
            {
                int val = _data.GetFaceValue(i);
                _faceImages[i].sprite = _faceSprites[val];
            }

            _durabilityView.SetDurability(_data.CurrentDurability, _data.MaxDurability);
        }
    }
}