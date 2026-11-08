using UnityEngine;

namespace _Scripts.Dices
{
    public class DiceFaceView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Face values")]
        [SerializeField] private Sprite[] _valuesSprites;
        [SerializeField] private SpriteRenderer _valueRenderer;
        
        [Header("Cracks")]
        [SerializeField] private SpriteRenderer _cracksRenderer;
        [SerializeField] private Sprite _lightCrackSprite;
        [SerializeField] private Sprite _heavyCrackSprite;
        [Space]
        [Range(0f, 1f)] [SerializeField] private float _lightCrackAlpha = 0.4f;
        [Range(0f, 1f)] [SerializeField] private float _heavyCrackAlpha = 0.7f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====        

        public void SetValue(int value)
        {
            _valueRenderer.sprite = _valuesSprites[value];
        }

        public void SetCracksLevel(int current, int max)
        {
            if (current >= max)
            {
                _cracksRenderer.gameObject.SetActive(false);
                return;
            }
            
            _cracksRenderer.gameObject.SetActive(true);

            var (sprite, alpha) = current switch
            {
                2 => (_lightCrackSprite, _lightCrackAlpha),
                _ => (_heavyCrackSprite, _heavyCrackAlpha)
            };

            _cracksRenderer.sprite = sprite;

            var color = _cracksRenderer.color;
            color.a = alpha;
            _cracksRenderer.color = color;
        }
    }
}