using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Combinations;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class CodexView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("References")]
        [SerializeField] private Image _backdropImage;
        [SerializeField] private CombinationDisplayInfoView _cardPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private CanvasGroup _cg;
        [SerializeField] private RectTransform _rect;
        
        [Header("Buttons")]
        [SerializeField] private Button _backdropButton;
        [SerializeField] private Button _hideButton;
        [SerializeField] private Button _showButton;
        
        [Header("Show")]
        [SerializeField] private float _showTime = 0.5f;
        [SerializeField] private Ease  _showEase = Ease.OutCubic; 
        
        [Header("Hide")]
        [SerializeField] private float _hideTime = 0.5f;
        [SerializeField] private Ease  _hideEase = Ease.OutCubic;
        
        [Header("Show cards")] 
        [SerializeField] private float _delayPerCard = 0.15f;
        [SerializeField] private float _scaleInTime = 0.5f;
        [SerializeField] private Ease _scaleInEase = Ease.OutBack;
        [SerializeField] private Ease _fadeInEase = Ease.InSine;

        private Tween _slideTween;
        private Tween _fadeTween;
        private float _backdropOpacity;
        private float _hiddenY;
        private Vector2 _hiddenPos;
        private readonly List<CombinationDisplayInfoView> _spawnedCards = new();
        private ActiveCombinationsService _combinationsService;
        private AudioFilterController _audioFilterController;

        [Inject]
        public void Construct(ActiveCombinationsService combinationsService, AudioFilterController audioFilterController)
        {
            _combinationsService = combinationsService;
            _audioFilterController = audioFilterController;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start()
        {
            _hiddenY = -_rect.rect.height;
            _hiddenPos = new Vector2(0, _hiddenY);
            _backdropOpacity = _backdropImage.color.a;
            HideInstant();
        }

        private void OnEnable()
        {
            _backdropButton.onClick.AddListener(Hide);
            _hideButton.onClick.AddListener(Hide);
            _showButton.onClick.AddListener(Show);
        }

        private void OnDisable()
        {
            _backdropButton.onClick.RemoveListener(Hide);
            _hideButton.onClick.RemoveListener(Hide);
            _showButton.onClick.RemoveListener(Show);
        }

        private async UniTaskVoid ShowCardsAsync()
        {
            ClearCards();
            
            foreach (var config in _combinationsService.ActiveCombinations.Reverse())
            {
                var card = Instantiate(_cardPrefab, _container);
                card.Setup(config);
                _spawnedCards.Add(card);
                
                card.transform.DoScaleIn(time: _scaleInTime, ease: _scaleInEase);
                card.PlayShowAnimation(_scaleInTime, _fadeInEase);
                
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerCard));
            }
        }

        private void ClearCards()
        {
            foreach (var card in _spawnedCards)
            {
                card.DOKill();
                Destroy(card.gameObject);
            }

            _spawnedCards.Clear();
        }

        private void Show()
        {
            SetShownState(true);
            KillTweens();
            
            _slideTween = _rect.DOAnchorPosY(0f, _showTime).SetEase(_showEase);
            _fadeTween = _backdropImage.DOFade(_backdropOpacity, _showTime).SetEase(_showEase);
            
            ShowCardsAsync().Forget();
        }

        private void Hide()
        {
            KillTweens();
            
            _slideTween = _rect.DOAnchorPosY(_hiddenY, _hideTime).SetEase(_hideEase);
            _fadeTween = _backdropImage.DOFade(0f, _hideTime).SetEase(_hideEase)
                .OnComplete(_cg.Hide);
            
            SetButtonsInteractable(false);
            _audioFilterController.ToggleFilter(false);
        }

        private void HideInstant()
        {
            SetShownState(false);
            KillTweens();
        }
        
        private void SetButtonsInteractable(bool canHide)
        {
            _hideButton.interactable = canHide;
            _backdropButton.interactable = canHide;
            _showButton.interactable = !canHide;
        }

        private void SetShownState(bool isShown)
        {
            if (isShown)
                _cg.Show();
            else 
                _cg.Hide();
            
            _rect.anchoredPosition = _hiddenPos;
            SetButtonsInteractable(isShown);
            _audioFilterController.ToggleFilter(isShown);
        }

        private void KillTweens()
        {
            _slideTween.Kill();
            _fadeTween.Kill();
        }
    }
}