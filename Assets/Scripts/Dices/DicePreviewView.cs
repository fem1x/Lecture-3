using _Scripts.Dices;
using _Scripts.Interfaces;
using _Scripts.Utility;
using UnityEngine;

namespace _Scripts.UI.Workshop
{
    public class DicePreviewView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [field: SerializeField] public Camera Camera { get; private set; }
        [field: SerializeField] public DiceModelDisplayView DisplayView { get; private set; }
        [field: SerializeField] public ModelRotator Rotator { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Bind(IReadOnlyDiceData data) => DisplayView.Bind(data);
        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}