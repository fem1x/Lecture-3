using System.Collections.Generic;
using System.Linq;
using _Scripts.Combinations;
using _Scripts.Dices;
using _Scripts.Managers;
using _Scripts.Utility.Extensions;
using TMPro;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class TotalSumView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _cg;

        private int _currentSum = -1;
        private Vector3 _defaultScale;
        private ActiveSelectionService _selectionService;
        
        [Inject]
        public void Construct(ActiveSelectionService selectionService)
        {
            _selectionService = selectionService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private void OnEnable() => _selectionService.OnSelectionEvaluated += UpdateSum;
        private void OnDisable() => _selectionService.OnSelectionEvaluated -= UpdateSum;

        private void Start()
        {
            _defaultScale = transform.localScale;
            _cg.Hide();
        }

        private void UpdateSum(List<Dice> dices, CombinationConfig _)
        {
            if (dices.Count == 0)
            {
                _cg.Hide();
                return;
            }

            _cg.Show();
            var sum = dices.Sum(d => d.Data.RolledValue);
            if (sum == _currentSum) return;

            _currentSum = sum;
            _text.text = sum.ToString();
            transform.localScale = _defaultScale;
            transform.DoScalePop(scaleMult: 1.15f, outTime: 0.18f);
        }
    }
}