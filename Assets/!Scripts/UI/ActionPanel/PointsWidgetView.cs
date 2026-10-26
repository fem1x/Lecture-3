using System;
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    public class PointsWidgetView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _multText;
        [SerializeField] private ActionButtonView _buttonView;
        
        private ActiveSelectionService _selectionService;

        [Inject]
        public void Construct(ActiveSelectionService selectionService)
        {
            _selectionService = selectionService;
            _selectionService.OnSelectionEvaluated += HandleSelection;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => SetEmpty();

        private void OnEnable() => _buttonView.SetDefaultAppearance();
        
        private void OnDestroy()
        {
            if (_selectionService != null)
                _selectionService.OnSelectionEvaluated -= HandleSelection;
        }

        private void HandleSelection(List<Dice> dices, CombinationConfig combo)
        {
            if (combo == null)
            {
                SetEmpty();
                return;
            }
            
            _pointsText.text = combo.BasePoints.ToString();
            _multText.text = combo.Multiplier.ToString();
            _buttonView.SetInteractable(true);
        }
        
        private void SetEmpty()
        {
            _pointsText.text = "0";
            _multText.text = "0";
            _buttonView.SetInteractable(false);
        }
    }
}