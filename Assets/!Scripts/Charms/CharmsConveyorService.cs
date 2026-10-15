using System;
using System.Collections.Generic;
using _Scripts.Managers;
using VContainer;

namespace _Scripts.Charms
{
    public class CharmsConveyorService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        //Constants
        private const int MaxCharmsCount = 3;
        private const int ReverseCost = 1;
        private const int ScrapReward = 2;
        
        //Events
        public event Action<int> OnConveyorMoved;
        public event Action OnStateChanged;
        public event Action OnChoiceCompleted;
        
        public int CurrentIndex { get; private set; }
        public CharmConfigBase CurrentCharm => _charms[CurrentIndex];

        private List<CharmConfigBase> _charms;
        
        #region DI
        private readonly RepairTokensManager _tokensManager;
        private readonly CharmsService _charmsService;
        
        [Inject]
        public CharmsConveyorService(RepairTokensManager tokensManager, CharmsService charmsService)
        {
            _tokensManager = tokensManager;
            _charmsService = charmsService;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void InitializeConveyor(List<CharmConfigBase> charms)
        {
            _charms = new List<CharmConfigBase>(charms);
            CurrentIndex = 0;
            
            OnConveyorMoved?.Invoke(CurrentIndex);
            OnStateChanged?.Invoke();
        }
        
        #region Conveyor Movement
        
        public void MoveNext()
        {
            if (!CanMoveNext) return;
            
            CurrentIndex++;
            OnConveyorMoved?.Invoke(CurrentIndex);
            OnStateChanged?.Invoke();
        }

        public void MoveReverse()
        {
            if (!CanMoveReverse) return;
            if (!_tokensManager.TrySpendTokens(ReverseCost)) return;
            
            CurrentIndex--;
            OnConveyorMoved?.Invoke(CurrentIndex);
            OnStateChanged?.Invoke();
        }

        public bool CanMoveNext => CurrentIndex < _charms.Count - 1;
        public bool CanMoveReverse => CurrentIndex > 0 && _tokensManager.CurrentTokens >=  ReverseCost;
        
        #endregion
        
        #region Selection

        public void Claim()
        {
            if (!_charmsService.TryAddCharm(CurrentCharm)) return;
            OnChoiceCompleted?.Invoke();
        }

        public void Scrap()
        {
            if (!CanScrap) return;
            _tokensManager.AddTokens(ScrapReward);
            OnChoiceCompleted?.Invoke();
        }

        public void Replace(Charm oldCharm)
        {
            _charmsService.ReplaceCharm(oldCharm, CurrentCharm);
            OnChoiceCompleted?.Invoke();
        }

        public bool CanScrap => CurrentIndex == _charms.Count - 1;

        #endregion
    }
}