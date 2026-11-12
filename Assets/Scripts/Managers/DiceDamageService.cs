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

        public DamageStep DamageDice(Dice dice, IReadOnlyList<Dice> allScoredDices, int amount = 1)
        {
            var damageContext = new DiceDamageContext(dice, amount, allScoredDices);

            foreach (var charm in _charmsService.ActiveCharms)
                charm.OnPreTakeDamage(damageContext);

            if (damageContext.ShouldForceDegrade)
            {
                return TryApplyDegrade(dice, restoreHp: true, notify: false)
                    ? new DamageStep(dice, DamageOutcome.Degraded)
                    : new DamageStep(dice, DamageOutcome.Blocked);
            }

            if (damageContext.IsCancelled)
                return new DamageStep(dice, DamageOutcome.Blocked);

            if (damageContext.DamageAmount <= 0)
                return new DamageStep(dice, DamageOutcome.None);

            var data = dice.Data;
            if (data.CurrentDurability - damageContext.DamageAmount <= 0)
            {
                return TryApplyDegrade(dice, restoreHp: true, notify: false)
                    ? new DamageStep(dice, DamageOutcome.Degraded)
                    : new DamageStep(dice, DamageOutcome.Blocked);
            }

            data.TakeDamage(damageContext.DamageAmount, notify: false);
            return new DamageStep(dice, DamageOutcome.Damaged);
        }

        public DamageStep DegradeDice(Dice dice, bool restoreHp = false)
        {
            return TryApplyDegrade(dice, restoreHp: restoreHp, notify: false)
                ? new DamageStep(dice, DamageOutcome.Degraded)
                : new DamageStep(dice, DamageOutcome.Blocked);
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