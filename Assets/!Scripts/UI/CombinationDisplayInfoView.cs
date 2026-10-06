using _Scripts.Configs;
using _Scripts.Dices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI
{
    public class CombinationDisplayInfoView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Combination")]
        [SerializeField] private CombinationType _combinationType;
        
        [Header("UI Texts")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _multiplierText;
        
        [Header("Dice Display")]
        [SerializeField] private Image[] _diceSlots;
        [SerializeField] private DiceAtlasConfig _diceAtlasConfig;
        
        [Header("Switch Animation")]
        [SerializeField] private float _switchInterval = 2.0f;
        
        private DicePatternExample[] _patterns;
        private int _currentPatternIndex;
        private CombinationScoreConfig _scoreConfig;
        private CombinationDisplayInfoConfig _displayInfoConfig;
        
        [Inject]
        public void Construct(CombinationScoreConfig scoreConfig, CombinationDisplayInfoConfig displayInfoConfig)
        {
            _scoreConfig = scoreConfig;
            _displayInfoConfig = displayInfoConfig;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Start() => Setup();
        private void OnDisable() => CancelInvoke(nameof(NextPattern));
        
        public void Setup()
        {
            //Score x Mult
            var scoreData = _scoreConfig.GetData(_combinationType);
            _titleText.text = scoreData.DisplayName;
            _pointsText.text = scoreData.BasePoints.ToString();
            _multiplierText.text = scoreData.Multiplier.ToString();

            var displayInfo = _displayInfoConfig.GetDisplayInfo(_combinationType);
            _descriptionText.text = displayInfo.Description;
            _patterns = displayInfo.Patterns;

            RestartPatternLoop();
        }
        
        private void RestartPatternLoop()
        {
            CancelInvoke(nameof(NextPattern));

            _currentPatternIndex = 0;
            ApplyPattern(_patterns[0]);

            InvokeRepeating(nameof(NextPattern), _switchInterval, _switchInterval);
        }

        private void NextPattern()
        {
            _currentPatternIndex = (_currentPatternIndex + 1) % _patterns.Length;
            ApplyPattern(_patterns[_currentPatternIndex]);
        }
        
        private void ApplyPattern(DicePatternExample pattern)
        {
            for (int i = 0; i < _diceSlots.Length; i++)
            {
                if (i < pattern.Values.Length)
                {
                    _diceSlots[i].gameObject.SetActive(true);
                    
                    var faceVal = pattern.Values[i];
                    _diceSlots[i].sprite = _diceAtlasConfig.GetSprite(faceVal);
                }
                else
                {
                    _diceSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}