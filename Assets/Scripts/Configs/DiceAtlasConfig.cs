using UnityEngine;

namespace _Scripts.Configs
{
    [CreateAssetMenu(fileName = "DiceAtlasConfig", menuName = "Configs/DiceAtlasConfig")]
    public class DiceAtlasConfig : ScriptableObject
    {
        [SerializeField] private Sprite[] _faceSprites;

        public Sprite GetSprite(int diceValue)
        {
            return _faceSprites[diceValue];
        }
    }
}