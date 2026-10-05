using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    public class CurrentCombinationView : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text _combinationNameText; 
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _multText;
        [Space]
        [SerializeField] private Button _submitButton;
        [Space]
        [SerializeField] private string _placeholderCombinationText = "Select Dice";

        private List<Dice> _dices;
        
        #region DI
        private DiceTableController _diceTableController;
        private IScoreCalculator _scoreCalculator;

        [Inject]
        public void Construct(DiceTableController diceTableController, IScoreCalculator scoreCalculator)
        {
            _diceTableController = diceTableController;
            _scoreCalculator = scoreCalculator;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void Start() => SetEmpty();
        
        public void Init(List<Dice> dices)
        {
            _dices = dices;
            
            foreach (var dice in _dices)
                dice.Data.OnLockChanged += HandleSelectionChanged;

            SetEmpty();
        }

        private void OnDestroy()
        {
            foreach (var dice in _dices)
                dice.Data.OnLockChanged -= HandleSelectionChanged;
        }
        
        private void HandleSelectionChanged(bool _)
        {
            var selectedDices = _diceTableController.GetSelectedDices();

            if (selectedDices.Count == 0)
            {
                SetEmpty();
                return;
            }
            
            var result = _scoreCalculator.Calculate(selectedDices);
            SetCombination(result.Data.DisplayName, result.Data.BasePoints, result.Data.Multiplier);
        }
        
        private void SetEmpty()
        {
            _combinationNameText.text = _placeholderCombinationText;
            _pointsText.text = "0";
            _multText.text = "0";
            
            _submitButton.interactable = false;
        }

        private void SetCombination(string name, int points, int mult)
        {
            _combinationNameText.text = name;
            _pointsText.text = points.ToString();
            _multText.text = mult.ToString();
            
            _submitButton.interactable = true;
        }
    }
}