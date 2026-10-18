using _Scripts.Dices;

namespace _Scripts.Data_Transfer.Contexts
{
    public class DiceDegradeContext
    {
        //In:
        public Dice Dice { get; }
        
        //Out:
        public bool PreventDegradeAndRestoreHealth { get; set; }

        public DiceDegradeContext(Dice dice)
        {
            Dice = dice;
        }
    }
}