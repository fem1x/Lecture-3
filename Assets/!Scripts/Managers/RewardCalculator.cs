using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;

namespace _Scripts.Managers
{
    public class RewardCalculator
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly ResourceManager _resourceManager;
        private readonly TokenRewardConfig _tokenRewardConfig;

        [Inject]
        public  RewardCalculator(ResourceManager resourceManager, TokenRewardConfig tokenRewardConfig)
        {
            _resourceManager = resourceManager;
            _tokenRewardConfig = tokenRewardConfig;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public TokenRewardData GetTokenRewards(int levelTokens)
        {
            var throwsLeft = _resourceManager.DiceCount;
            var rerollsLeft = _resourceManager.RerollsCount;

            var throwsTokens = throwsLeft / _tokenRewardConfig.ThrowsPerToken;
            var rerollsTokens = rerollsLeft / _tokenRewardConfig.RerollsPerToken;
            
            return new TokenRewardData(levelTokens, throwsLeft, throwsTokens, rerollsLeft, rerollsTokens);
        }
    }
}