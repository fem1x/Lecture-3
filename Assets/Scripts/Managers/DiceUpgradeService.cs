using _Scripts.Configs;
using _Scripts.Dices;
using VContainer;
using UnityEngine;

namespace _Scripts.Managers
{
    public class DiceUpgradeService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly DiceUpgradeConfig _config;
        private readonly RepairTokensManager _tokensManager;

        [Inject]
        public DiceUpgradeService(DiceUpgradeConfig config, RepairTokensManager tokensManager)
        {
            _config = config;
            _tokensManager = tokensManager;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public int GetUpgradeCost(DiceData diceData)
        {
            return _config.BaseCost + diceData.UpgradeCount * _config.CostIncrease;
        }

        public bool TryUpgradeFace(DiceData diceData, int faceIndex, out int bonus)
        {
            bonus = 0;

            var cost = GetUpgradeCost(diceData);
            if (!_tokensManager.TrySpendTokens(cost))
                return false;

            bonus = GenerateBonus();
            diceData.AddFaceBonus(faceIndex, bonus);

            return true;
        }

        private int GenerateBonus()
        {
            if (_config.MinBonus == _config.MaxBonus)
                return _config.MinBonus;

            var mean = (_config.MinBonus + _config.MaxBonus) / 2f;
            var standardDeviation = (_config.MaxBonus - _config.MinBonus) / 6f;

            var u1 = Mathf.Max(Random.value, 0.0001f);
            var u2 = Random.value;

            var normalRandom = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
            var value = mean + normalRandom * standardDeviation;

            return Mathf.Clamp(
                Mathf.RoundToInt(value),
                _config.MinBonus,
                _config.MaxBonus);
        }
    }
}