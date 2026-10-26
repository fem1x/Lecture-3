using System;
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Configs;
using _Scripts.Dices;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public enum LockBreakResult
    {
        Success,
        Forced
    }
    
    public class CodeLockService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public event Action<CombinationConfig> OnLockSet;
        public event Action OnLockBroken;
        
        public CombinationConfig CurrentLock { get; private set; }
        public bool IsActive => CurrentLock != null;
        
        private readonly DiceDamageService _damageService;

        [Inject]
        public CodeLockService(DiceDamageService damageService)
        {
            _damageService = damageService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void SetLock(LevelConfig levelConfig)
        {
            CurrentLock = levelConfig.HasLock ? levelConfig.GetRandomLock() : null;
            OnLockSet?.Invoke(CurrentLock);
        }

        public LockBreakResult EvaluateAttempt(List<Dice> dices, CombinationConfig playedCombo)
        {
            if (playedCombo == CurrentLock)
                return LockBreakResult.Success;

            if (dices.Count == 5)
                return LockBreakResult.Forced;

            Debug.LogError("LockBreakResult error! (wrong combination and <5 dice");
            return default;
        }

        public void BreakLock(LockBreakResult result, List<Dice> dices)
        {
            if (result == LockBreakResult.Forced)
                _damageService.DamageDices(dices);

            CurrentLock = null;
            OnLockBroken?.Invoke();
        }
        
        public bool IsValidAttempt(int diceCount, CombinationConfig playedCombo)
        {
            if (!IsActive) return true;
            return playedCombo == CurrentLock || diceCount == 5;
        }
    }
}