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

        public void DamageDices(IReadOnlyList<Dice> dices, int amount = 1)
        {
            foreach (var dice in dices)
            {
                var damageContext = new DiceDamageContext(dice, amount, dices);
        
                foreach (var charm in _charmsService.ActiveCharms)
                    charm.OnPreTakeDamage(damageContext);
        
                if (damageContext.ShouldForceDegrade)
                {
                    ApplyDegrade(dice);
                    continue;
                }

                if (damageContext.IsCancelled || damageContext.DamageAmount <= 0)
                    continue;
        
                ApplyDamage(dice, damageContext.DamageAmount);
            }
        }

        public void DegradeDices(IReadOnlyList<Dice> dices, int amount = 1)
        {
            foreach (var dice in dices)
            {
                ApplyDegrade(dice, amount, false);
            }
        }


        private void ApplyDamage(Dice dice, int amount = 1)
        {
            var data = dice.Data;

            if (data.CurrentDurability - amount <= 0)
            {
                ApplyDegrade(dice);
                return;
            }
    
            data.TakeDamage(amount);
        }

        private void ApplyDegrade(Dice dice, int amount = 1, bool restoreHp = true)
        {
            var degradeContext = new DiceDegradeContext(dice);
    
            foreach (var charm in _charmsService.ActiveCharms)
                charm.OnDiceDegrading(degradeContext);

            if (degradeContext.IsCancelled)
                return;
            
            dice.Data.Degrade(restoreHp);
        }
    }
}