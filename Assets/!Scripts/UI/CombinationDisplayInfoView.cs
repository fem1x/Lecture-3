using _Scripts.Combinations;
using _Scripts.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class CombinationDisplayInfoView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Config")]
        [SerializeField] private CombinationConfig _config;
        
        [Header("UI Texts")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _pointsText;
        [SerializeField] private TMP_Text _multiplierText;
        
        [Header("Dice Display")]
        [SerializeField] private Image[] _diceSlots;
        [SerializeField] private DiceAtlasConfig _diceAtlasConfig;
        
        [Header("Switch Animation")]
        [SerializeField] private float _switchInterval = 1.5f;
        
        private int _currentPatternIndex;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        private void Start()
        {
            if (_config != null)
                Setup(_config);
        }

        private void OnDisable() => CancelInvoke(nameof(NextPattern));
        
        public void Setup(CombinationConfig config)
        {
            _config = config;
            if (_config == null) return;

            _titleText.text = _config.DisplayName;
            _descriptionText.text = _config.Description;
            _pointsText.text = _config.BasePoints.ToString();
            _multiplierText.text = _config.Multiplier.ToString();

            RestartPatternLoop();
        }
        
        private void RestartPatternLoop()
        {
            CancelInvoke(nameof(NextPattern));

            _currentPatternIndex = 0;
            ApplyPattern(_config.Patterns[0]);

            InvokeRepeating(nameof(NextPattern), _switchInterval, _switchInterval);
        }

        private void NextPattern()
        {
            if (_config.Patterns == null || _config.Patterns.Length == 0) return;

            _currentPatternIndex = (_currentPatternIndex + 1) % _config.Patterns.Length;
            ApplyPattern(_config.Patterns[_currentPatternIndex]);
        }
        
        private void ApplyPattern(DicePatternExample pattern)
        {
            for (int i = 0; i < _diceSlots.Length; i++)
            {
                if (pattern.Values != null && i < pattern.Values.Length)
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