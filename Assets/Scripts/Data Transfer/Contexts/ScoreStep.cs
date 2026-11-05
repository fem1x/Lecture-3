using _Scripts.UI.Charms;
using UnityEngine;

namespace _Scripts.Data_Transfer.Contexts
{
    public enum ScoreStepType
    {
        CombinationBase,
        DiceScored,
        CharmTrigger,
        Multiply
    }
    
    public readonly struct ScoreStep
    {
        public ScoreStepType Type { get; }
        public int BonusPoints { get; }
        public int BonusMultiplier { get; }
        public float XMultiplier { get; }
        public Transform SourceTransform { get; }
        public CharmEquippedView CharmView { get; }

        public ScoreStep(
            ScoreStepType type, 
            int bonusPoints = 0, 
            int bonusMultiplier = 0, 
            float xMultiplier = 1f,
            Transform sourceTransform = null, 
            CharmEquippedView charmView = null)
        {
            Type = type;
            BonusPoints = bonusPoints;
            BonusMultiplier = bonusMultiplier;
            XMultiplier = xMultiplier;
            SourceTransform = sourceTransform;
            CharmView = charmView;
        }
    }
}