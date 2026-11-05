using _Scripts.Managers;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class DiceCounterView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _text;
        
        private ResourceManager _resourceManager;

        [Inject]
        public void Construct(ResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void Start() => UpdateView(_resourceManager.DiceCount);
        private void OnEnable() => _resourceManager.OnDicesChanged += UpdateView;
        private void OnDisable() => _resourceManager.OnDicesChanged -= UpdateView;


        private void UpdateView(int count)
        {
            _text.text = count.ToString();
        }
    }
}