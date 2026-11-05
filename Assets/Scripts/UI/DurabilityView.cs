using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class DurabilityView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private Image _cracksOverlay;
        [Space]
        [SerializeField] private Sprite _lightCrackSprite;
        [SerializeField] private Sprite _heavyCrackSprite;
        [Space]
        [Range(0f, 1f)] [SerializeField] private float _lightCrackAlpha = 0.4f;
        [Range(0f, 1f)] [SerializeField] private float _heavyCrackAlpha = 0.7f;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetDurability(int current, int max)
        {
            if (current >= max)
            {
                _cracksOverlay.gameObject.SetActive(false);
                return;
            }

            _cracksOverlay.gameObject.SetActive(true);

            var (sprite, alpha) = current switch
            {
                2 => (_lightCrackSprite, _lightCrackAlpha),
                _ => (_heavyCrackSprite, _heavyCrackAlpha)
            };

            _cracksOverlay.sprite = sprite;

            var color = _cracksOverlay.color;
            color.a = alpha;
            _cracksOverlay.color = color;
        }
    }
}