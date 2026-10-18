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
        public List<CharmConfigBase> Charms { get; private set; } = new();
        public CharmConfigBase CurrentCharm => Charms[CurrentIndex];

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
            Charms = new List<CharmConfigBase>(charms);
            CurrentIndex = 0;
            
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

        public bool CanMoveNext => CurrentIndex < Charms.Count - 1;
        public bool CanMoveReverse => CurrentIndex > 0 && _tokensManager.CurrentTokens >=  ReverseCost;
        
        #endregion
        
        #region Selection

        public bool TryClaim(CharmConfigBase config, out CharmBase createdCharm)
        {
            if (!_charmsService.TryAddCharm(config, out createdCharm))
                return false;

            OnChoiceCompleted?.Invoke();
            return true;
        }

        public void Scrap()
        {
            if (!CanScrap) return;
            _tokensManager.AddTokens(ScrapReward);
            OnChoiceCompleted?.Invoke();
        }

        public void Replace(CharmBase oldCharm)
        {
            _charmsService.ReplaceCharm(oldCharm, CurrentCharm);
            OnChoiceCompleted?.Invoke();
        }

        public bool CanScrap => CurrentIndex == Charms.Count - 1;

        #endregion
    }
}