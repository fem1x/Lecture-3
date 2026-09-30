using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Configs;
using _Scripts.Utility;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace _Scripts.Dices
{
    public class DiceRoller
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly DiceRollConfig _config;
        
        [Inject]
        public DiceRoller(DiceRollConfig config)
        {
            _config = config;
        }
        
        public bool IsRolling { get; private set; }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        #region Public API
        
        public async UniTask FirstRollAsync(List<Dice> dices)
        {
            if (IsRolling) return;
            await ExecuteRollAsync(dices, _config.FirstRollSettings);
        }

        public async UniTask RerollAsync(List<Dice> dices)
        {
            if (IsRolling || dices == null || dices.Count == 0) return;
            await ExecuteRollAsync(dices, _config.RerollSettings);
        }
        
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        
        private async UniTask ExecuteRollAsync(List<Dice> dices, DiceThrowSettings settings)
        {
            IsRolling = true;

            try
            {
                foreach (var dice in dices)
                {
                    if (dice == null) continue;

                    DiceThrow(dice, settings);
                    if (settings.TimeBetweenThrows > 0f)
                        await UniTask.Delay(TimeSpan.FromSeconds(settings.TimeBetweenThrows));
                }

                await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
                await UniTask.WaitUntil(() => dices.All(d => d == null || d.IsStopped));

                foreach (var dice in dices)
                {
                    if (dice != null)
                        dice.UpdateValue();
                }
            }
            finally
            {
                IsRolling = false;
            }
        }

        private void DiceThrow(Dice dice, DiceThrowSettings settings)
        {
            float forceMag = Utils.NumberInRange(settings.Force, settings.ForceRandomize);
            float torqueMag = Utils.NumberInRange(settings.Torque, settings.TorqueRandomize);

            Vector3 dir = settings.Direction.normalized;
            if (settings.DirectionRandomize > 0f)
            {
                Vector3 offset = Random.insideUnitSphere * settings.DirectionRandomize;
                dir = (dir + offset).normalized;
            }

            Vector3 force = dir * forceMag;
            Vector3 torque = Random.insideUnitSphere.normalized * torqueMag;

            dice.Roll(force, torque);
        }
    }
}