using System;
using System.Collections.Generic;
using _Scripts.Configs;
using UnityEngine;

namespace _Scripts.Managers
{
    public class MapService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnMapRequested;
        public event Action<int> OnSectionChanged; //(ActiveSectionIndex)
        public event Action OnMapInitialized;

        public List<LevelSection> SectionsList => _levelService.SectionsList;
        public int SelectedSection { get; private set; } = 1;
        public bool CanMoveDown => SelectedSection < SectionsList.Count;
        public bool CanMoveUp => SelectedSection >= 2;
        
        private readonly LevelService _levelService;

        public MapService(LevelService levelService)
        {
            _levelService = levelService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void Initialize()
        {
            SelectedSection = 1;   
            OnMapInitialized?.Invoke();
        }
        
        public void MoveDown()
        {
            if (!CanMoveDown) return;
            
            SelectedSection++;
            OnSectionChanged?.Invoke(SelectedSection);
        }

        public void MoveUp()
        {
            if (!CanMoveUp) return;
            SelectedSection--;
            OnSectionChanged?.Invoke(SelectedSection);
        }
        
        public void RequestMap() => OnMapRequested?.Invoke();
    }
}