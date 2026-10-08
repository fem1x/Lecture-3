using _Scripts.Charms;
using UnityEngine;

namespace _Scripts.Structs___Enums.Contexts
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
        public Transform SourceTransform { get; }
        public CharmView CharmView { get; }

        public ScoreStep(
            ScoreStepType type, 
            int bonusPoints = 0, 
            int bonusMultiplier = 0, 
            Transform sourceTransform = null, 
            CharmView charmView = null)
        {
            Type = type;
            BonusPoints = bonusPoints;
            BonusMultiplier = bonusMultiplier;
            SourceTransform = sourceTransform;
            CharmView = charmView;
        }
    }
}