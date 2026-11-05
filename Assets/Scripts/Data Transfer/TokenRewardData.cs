namespace _Scripts.Dices
{
    public readonly struct TokenRewardData
    {
        public readonly int LevelTokens;
        public readonly int ThrowsLeft;
        public readonly int ThrowsTokens;
        public readonly int RerollsLeft;
        public readonly int RerollsTokens;

        public int TotalTokens => LevelTokens + ThrowsTokens + RerollsTokens;

        public TokenRewardData(int levelTokens, int throwsLeft, int throwsTokens, int rerollsLeft, int rerollsTokens)
        {
            LevelTokens = levelTokens;
            ThrowsLeft = throwsLeft;
            ThrowsTokens = throwsTokens;
            RerollsLeft = rerollsLeft;
            RerollsTokens = rerollsTokens;
        }
    }
}