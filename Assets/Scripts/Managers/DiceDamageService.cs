using System.Collections.Generic;
using _Scripts.Charms;
using _Scripts.Data_Transfer.Contexts;
using _Scripts.Dices;
using VContainer;

namespace _Scripts.Managers
{
    public class DiceDamageService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private readonly CharmsService _charmsService;

        [Inject]
        public DiceDamageService(CharmsService charmsService)
        {
            _charmsService = charmsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        public DamageSequencePlan DamageDices(IReadOnlyList<Dice> dices, int amount = 1)
        {
            var plan = new DamageSequencePlan();

            foreach (var dice in dices)
            {
                var damageContext = new DiceDamageContext(dice, amount, dices);

                foreach (var charm in _charmsService.ActiveCharms)
                    charm.OnPreTakeDamage(damageContext);

                if (damageContext.ShouldForceDegrade)
                {
                    if (TryApplyDegrade(dice, notify: false))
                        plan.Steps.Add(new DamageStep(dice, 0, isDegraded: true));
                    continue;
                }

                if (damageContext.IsCancelled || damageContext.DamageAmount <= 0)
                    continue;

                var data = dice.Data;
                if (data.CurrentDurability - damageContext.DamageAmount <= 0)
                {
                    if (TryApplyDegrade(dice, notify: false))
                        plan.Steps.Add(new DamageStep(dice, damageContext.DamageAmount, isDegraded: true));
                }
                else
                {
                    data.TakeDamage(damageContext.DamageAmount, notify: false);
                    plan.Steps.Add(new DamageStep(dice, damageContext.DamageAmount, isDegraded: false));
                }
            }

            return plan;
        }

        public void DegradeDices(IReadOnlyList<Dice> dices, int amount = 1)
        {
            foreach (var dice in dices)
            {
                TryApplyDegrade(dice, restoreHp: false, notify: true);
            }
        }

        private bool TryApplyDegrade(Dice dice, bool restoreHp = true, bool notify = true)
        {
            var degradeContext = new DiceDegradeContext(dice);

            foreach (var charm in _charmsService.ActiveCharms)
                charm.OnDiceDegrading(degradeContext);

            if (degradeContext.IsCancelled)
                return false;

            dice.Data.Degrade(restoreHp, notify);
            return true;
        }
    }
}