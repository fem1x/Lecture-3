using System;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class BackdropView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _image;
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease _ease;
        [SerializeField] private float _opacity = 0.85f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Awake()
        {
            _image.SetVisible(false);
            _image.raycastTarget = false;
        }

        public async UniTask ShowAsync()
        {
            _image.raycastTarget = true;
            await _image.DOFade(_opacity, _showTime).SetEase(_ease);
        }

        public async UniTask HideAsync()
        {
            _image.raycastTarget = false;
            await _image.DOFade(0, _showTime).SetEase(_ease);   
        }
    }
}