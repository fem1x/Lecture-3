using _Scripts.Dices;
using _Scripts.Interfaces;
using VContainer;

namespace _Scripts.Managers
{
    public class DiceRepairService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private const int RepairCost = 1;
        
        private readonly RepairTokensManager _tokensManager;
        private readonly DiceTableController _tableController;
        private readonly ISfxPlayer _sfxPlayer;
        
        [Inject]
        public DiceRepairService(
            RepairTokensManager tokensManager, 
            DiceTableController tableController,
            ISfxPlayer sfxPlayer)
        {
            _tokensManager = tokensManager;
            _tableController = tableController;
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public bool CanRepair(IReadOnlyDiceData dice) => 
            (dice.IsDamaged || dice.CurrentDurability < dice.MaxDurability) 
            && _tokensManager.CurrentTokens >= RepairCost;

        public bool TryRepair(IReadOnlyDiceData dice)
        {
            if (!CanRepair(dice)) 
                return false;

            if (!_tokensManager.TrySpendTokens(RepairCost)) 
                return false;

            var targetDice = _tableController.GetDiceById(dice.Id);
            targetDice.Data.Repair();
            _sfxPlayer.Play("Repair");            
            return true;
        }
    }
}