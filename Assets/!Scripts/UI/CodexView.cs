using System.Collections.Generic;
using System.Linq;
using _Scripts.Combinations;
using UnityEngine;
using VContainer;

namespace _Scripts.UI
{
    public class CodexView : MonoBehaviour
    {
        [SerializeField] private CombinationDisplayInfoView _cardPrefab;
        [SerializeField] private Transform _container;

        private readonly List<CombinationDisplayInfoView> _spawnedCards = new();

        private ActiveCombinationsService _combinationsService;

        [Inject]
        public void Construct(ActiveCombinationsService combinationsService)
        {
            _combinationsService = combinationsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        private void OnEnable() => Rebuild();

        private void Rebuild()
        {
            foreach (var card in _spawnedCards)
                Destroy(card.gameObject);

            _spawnedCards.Clear();

            foreach (var config in _combinationsService.ActiveCombinations.Reverse())
            {
                var card = Instantiate(_cardPrefab, _container);
                card.Setup(config);
                _spawnedCards.Add(card);
            }
        }
    }
}