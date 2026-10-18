using _Scripts.UI.Charms;

namespace _Scripts.Data_Transfer.Contexts
{
    public struct CharmTriggerResult
    {
        public CharmEquippedView View { get; }
        public int BonusPoints { get; }
        public int BonusMultiplier { get; }
        public float XMultiplier { get; }

        public CharmTriggerResult(CharmEquippedView view, int bonusPoints = 0, int bonusMultiplier = 0, float xMultiplier = 1f)
        {
            View = view;
            BonusPoints = bonusPoints;
            BonusMultiplier = bonusMultiplier;
            XMultiplier = xMultiplier;
        }
    }
}