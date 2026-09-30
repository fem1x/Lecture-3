using System;
using System.Linq;
using _Scripts.Configs;
using _Scripts.Dices;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Playables;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class CombinationButtonView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public static event Action<FoundCombination> OnAnyButtonClicked;
        
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        
        private FoundCombination _combination;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void OnEnable() => _button.onClick.AddListener(HandleClick);
        private void OnDisable() => _button.onClick.RemoveListener(HandleClick);

        public void Setup(FoundCombination combination, CombinationScoreConfig scoreConfig)
        {
            _combination = combination;
            var displayName = scoreConfig.GetData(combination.Type).DisplayName;
            var basePoints = scoreConfig.GetData(combination.Type).BasePoints;
            var multiplier = scoreConfig.GetData(combination.Type).Multiplier;
            
            var valuesString = string.Join(", ", combination.Dices.Select(d => d.Value));   
            _titleText.text = $"{displayName} ({valuesString})";
            _scoreText.text = $"{basePoints} x {multiplier}";
        }
        
        private void HandleClick()
        {
            OnAnyButtonClicked?.Invoke(_combination);
        }
    }
}