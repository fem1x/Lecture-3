using _Scripts.Data_Transfer.Contexts;

namespace _Scripts.Charms
{
    public class MatryoshkaCharm : CharmBase
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public MatryoshkaCharm(MatryoshkaCharmConfig config) : base(config) { }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public override void OnCombinationScored(CombinationScoreContext context)
        {
            if (!context.Combination.IsSet)
                return;

            var dices = context.SelectedDices;
            var bonusMultiplier = dices[0].Data.RolledValue * dices.Count;
            context.AddTrigger(View, multiplier: bonusMultiplier);
        }
    }
}
