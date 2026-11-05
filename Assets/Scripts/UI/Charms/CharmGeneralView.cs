using _Scripts.Charms;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Charms
{
    public class CharmGeneralView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _iconImage;
        public CharmConfigBase Config { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Setup(CharmConfigBase config)
        {
            Config = config;
            SetIconSprite(Config.Icon);
        }

        public void SetIconSprite(Sprite sprite) => _iconImage.sprite = sprite;
    }
}