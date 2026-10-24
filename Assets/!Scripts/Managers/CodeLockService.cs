using System;
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;

namespace _Scripts.Managers
{
    public class CodeLockService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<CombinationConfig> OnLockSet;
        public event Action OnLockBroken;
        
        private CombinationConfig _currentLock;

        private readonly DiceDamageService _damageService;

        [Inject]
        public CodeLockService(DiceDamageService damageService)
        {
            _damageService = damageService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetLock(LevelConfig levelConfig)
        {
            _currentLock = levelConfig.HasLock ? levelConfig.GetRandomLock() : null;
        }

        public bool TryBreakLock(List<Dice> dices, CombinationConfig playedCombo)
        {
            if (playedCombo == _currentLock)
            {
                BreakLock();
                return true;
            }

            if (dices.Count == 5)
            {
                _damageService.DamageDices(dices);
                BreakLock();
                return true;
            }

            return false;
        }

        private void BreakLock()
        {
            _currentLock = null;
        }
    }
}