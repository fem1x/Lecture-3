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
        public event Action OnConveyorInitialized;
        public event Action<int> OnConveyorMoved;
        public event Action OnStateChanged;
        public event Action OnChoiceCompleted;
        
        public int CurrentIndex { get; private set; }
        public List<CharmConfigBase> Charms { get; private set; } = new();
        public CharmConfigBase CurrentCharm => Charms[CurrentIndex];

        private bool _isChoiceOpen;

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
            _isChoiceOpen = Charms.Count > 0;
            
            OnConveyorInitialized?.Invoke();
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

        public bool CanMoveNext => _isChoiceOpen && CurrentIndex < Charms.Count - 1;
        public bool CanMoveReverse => _isChoiceOpen && CurrentIndex > 0 && _tokensManager.CurrentTokens >= ReverseCost;
        
        #endregion
        
        #region Selection

        public bool TryClaim(CharmConfigBase config, out CharmBase createdCharm)
        {
            if (!_charmsService.TryAddCharm(config, out createdCharm))
                return false;

            CompleteChoice();
            return true;
        }

        public bool TryScrap(CharmConfigBase config)
        {
            if (!_isChoiceOpen || config != CurrentCharm) return false;

            _isChoiceOpen = false;
            _tokensManager.AddTokens(ScrapReward);
            OnChoiceCompleted?.Invoke();
            return true;
        }

        public CharmBase Replace(CharmBase oldCharm, CharmConfigBase newCharmConfig)
        {
            var newCharm = _charmsService.ReplaceCharm(oldCharm, newCharmConfig);
            CompleteChoice();
            return newCharm;
        }

        private void CompleteChoice()
        {
            _isChoiceOpen = false;
            OnChoiceCompleted?.Invoke();
        }

        #endregion
    }
}
