using System;
using System.Collections.Generic;
using _Scripts.Configs;

namespace _Scripts.Managers
{
    public class MapService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<int> OnSectionChanged; //(ActiveSectionIndex)
        public event Action OnMapInitialized;

        public List<LevelSection> SectionsList => _levelService.SectionsList;
        public int ActiveSectionIndex { get; private set; } = 1;
        public bool CanMoveDown => ActiveSectionIndex < SectionsList.Count;
        public bool CanMoveUp => ActiveSectionIndex >= 2;
        
        private readonly LevelService _levelService;

        public MapService(LevelService levelService)
        {
            _levelService = levelService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Initialize()
        {
            ActiveSectionIndex = 1;   
            OnMapInitialized?.Invoke();
        }
        
        public void MoveDown()
        {
            if (!CanMoveDown) return;
            
            ActiveSectionIndex++;
            OnSectionChanged?.Invoke(ActiveSectionIndex);
        }

        public void MoveUp()
        {
            if (!CanMoveUp) return;
            ActiveSectionIndex--;
            OnSectionChanged?.Invoke(ActiveSectionIndex);
        }
    }
}