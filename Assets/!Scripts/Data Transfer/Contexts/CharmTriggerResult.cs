using _Scripts.Charms;
using _Scripts.UI.Charms;

namespace _Scripts.Structs___Enums.Contexts
{
    public struct CharmTriggerResult
    {
        public CharmView View { get; }
        public int BonusPoints { get; }
        public int BonusMultiplier { get; }

        public CharmTriggerResult(CharmView view, int bonusPoints, int bonusMultiplier)
        {
            View = view;
            BonusPoints = bonusPoints;
            BonusMultiplier = bonusMultiplier;
        }
    }
}