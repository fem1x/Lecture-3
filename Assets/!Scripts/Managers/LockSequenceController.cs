using System;
using System.Collections.Generic;
using _Scripts.Dices;
using _Scripts.UI.CodeLock;
using _Scripts.Utility.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace _Scripts.Managers
{
    public class LockSequenceController : MonoBehaviour
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [Header("Slots")]
        [SerializeField] private List<CodeLockSlotView> _slots;
        [SerializeField] private float _stampDelay = 0.15f;
        [SerializeField] private float _delayPerDice = 0.5f;
        

        private ISfxPlayer _sfxPlayer;

        [Inject]
        public void Construct(ISfxPlayer sfxPlayer)
        {
            _sfxPlayer = sfxPlayer;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public async UniTask PlaySequenceAsync(List<Dice> dices)
        {
            ClearSlots();

            for (int i = 0; i < dices.Count && i < _slots.Count; i++)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_delayPerDice));
                
                dices[i].transform.DoScalePop().ToUniTask().Forget();
                
                await UniTask.Delay(TimeSpan.FromSeconds(_stampDelay));
                
                _slots[i].InsertValue(dices[i].Data.RolledValue);
                _slots[i].DiceImage.transform.DoScalePop().ToUniTask().Forget();;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.8f));
        }
        
        private void ClearSlots()
        {
            foreach (var slot in _slots)
                slot.RemoveValue();
        }
    }
}