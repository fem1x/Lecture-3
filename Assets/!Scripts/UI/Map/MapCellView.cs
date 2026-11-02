using System;
using _Scripts.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Setup(LevelConfig config)
        {
            _text.text = config.LevelIndex.ToString();
            
            var visuals = config.HasLock ? _codeLockLevel : _defaultLevel;
            _outlineImg.color = visuals.OutlineColor;
            _fillImg.color = visuals.FillColor;
            _crossImg.color = visuals.TextColor;
        }
    }
}