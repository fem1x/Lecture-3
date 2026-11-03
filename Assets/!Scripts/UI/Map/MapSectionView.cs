using System.Collections.Generic;
using _Scripts.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Scripts.UI
{
    public class MapSectionView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private MapCellView _mapCellPrefab;
        [SerializeField] private RectTransform _rect;

        private readonly List<MapCellView> _cellViews = new();
        private IObjectResolver _resolver;
        
        [Inject]
        public void Construct(IObjectResolver resolver) => _resolver = resolver;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Setup(LevelSection section)
        {
            for (int i = 0; i < section.Levels.Count; i++)
            {
                var cell = _resolver.Instantiate(_mapCellPrefab, _rect);
                cell.Setup(section.Levels[i], i + 1);
                _cellViews.Add(cell);
            }
        }
    }
}