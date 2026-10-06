using _Scripts.Dices;
using TMPro;
using UnityEngine;

namespace _Scripts.UI
{
    public class TokenRewardsView : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [SerializeField] private TMP_Text _totalTokensText;
        [SerializeField] private TMP_Text _levelTokensText;
        [SerializeField] private TMP_Text _throwsTokensText;
        [SerializeField] private TMP_Text _rerollsTokensText;
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        
        public void Setup(TokenRewardData data)
        {
            _totalTokensText.text = $"+ {data.TotalTokens}";
            _levelTokensText.text = data.LevelTokens.ToString();
            _throwsTokensText.text = data.ThrowsTokens.ToString();
            _rerollsTokensText.text = data.RerollsTokens.ToString();
        }
    }
}