using System;
using System.Collections.Generic;
using _Scripts.Configs;
using UnityEngine;

namespace _Scripts.Managers
{
    public class LevelService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action OnLevelChanged;
        
        public int CurrentLevelIndex {get; private set;}
        public int CurrentSectionIndex {get; private set;}
        
        public List<LevelSection> SectionsList => _levelsList.Sections;
        public int SectionsCount => SectionsList.Count;
        public int LevelsCount => SectionsList[CurrentSectionIndex - 1].Levels.Count;
        
        public LevelConfig CurrentLevel => 
            SectionsList[CurrentSectionIndex - 1].Levels[CurrentLevelIndex - 1];
        
        private readonly LevelsListConfig _levelsList;
        
        public LevelService(LevelsListConfig levelsList)
        {
            _levelsList = levelsList;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void InitializeLevelsOrder()
        {
            int index = 1;
            foreach (var section in _levelsList.Sections)
            {
                foreach (var level in section.Levels)
                {
                    level.OrderIndex = index;
                    index++;
                }
            }
        }
        
        public bool TrySelectNextLevel()
        {
            CurrentLevelIndex++;
            if (CurrentLevelIndex > SectionsList[CurrentSectionIndex].Levels.Count)
            {
                CurrentSectionIndex++;
                if (CurrentSectionIndex > SectionsCount)
                {
                    Debug.LogWarning("No more levels available!");
                    return false;
                }

                CurrentLevelIndex = 1;
            }
            
            OnLevelChanged?.Invoke();
            return true;
        }

        public void SelectLevel(int section, int level)
        {
            if (section > SectionsCount) return;
            if (level > SectionsList[section].Levels.Count) return;
            
            CurrentSectionIndex = section;
            CurrentLevelIndex = level;
            
            OnLevelChanged?.Invoke();
        }

        public void SelectFirstLevel() => SelectLevel(1, 1);
    }
}