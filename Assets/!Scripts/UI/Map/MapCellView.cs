using System;
using _Scripts.Configs;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    [Serializable]
    public struct LevelTypeVisuals
    {
        [field: SerializeField] public Color OutlineColor { get; private set; }
        [field: SerializeField] public Color FillColor { get; private set; }
        [field: SerializeField] public Color TextColor { get; private set; }
    }
    
    public class MapCellView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _outlineImg;
        [SerializeField] private Image _fillImg;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _crossImg;
        
        [Header("Level Types")] 
        [SerializeField] private LevelTypeVisuals _defaultLevel;
        [SerializeField] private LevelTypeVisuals _codeLockLevel;

        private int _orderIndex;
        private bool _isCompleted;
        private LevelService _levelService;
        
        [Inject]
        public void Construct(LevelService levelService) => _levelService = levelService;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable() => _levelService.OnLevelChanged += UpdateCompletedState;
        private void OnDisable() => _levelService.OnLevelChanged -= UpdateCompletedState;
        
        public void Setup(LevelConfig config, int index)
        {
            _orderIndex = config.OrderIndex;
            
            var visuals = config.HasLock ? _codeLockLevel : _defaultLevel;
            _crossImg.color = visuals.OutlineColor;
            _outlineImg.color = visuals.OutlineColor;
            _fillImg.color = visuals.FillColor;
            _text.color = visuals.TextColor;
            
            _text.text = index.ToString();
        }

        private void UpdateCompletedState()
        {
            if (_isCompleted) return;
            var newOrderIndex = _levelService.CurrentLevel.OrderIndex;
            _isCompleted = _orderIndex < newOrderIndex;
            if (!_isCompleted) return;
            
            _crossImg.gameObject.SetActive(_isCompleted);
            _crossImg.transform.DoScaleIn(time: 0.5f, ease: Ease.OutBack);
        }
    }
}