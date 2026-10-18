using System;
using System.Collections.Generic;
using UnityEngine.PlayerLoop;
using VContainer;
using VContainer.Unity;

namespace _Scripts.Charms
{
    public class CharmsPoolService : IInitializable, IDisposable
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        public List<CharmConfigBase> CharmsPool { get; private set; } = new();
        
        #region DI
        private readonly CharmsPoolConfig _charmsPoolConfig;
        private readonly CharmsService _charmsService;

        [Inject]
        public CharmsPoolService(CharmsPoolConfig charmsPoolConfig, CharmsService charmsService)
        {
            _charmsPoolConfig = charmsPoolConfig;
            _charmsService = charmsService;
        }
        #endregion
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public void InitializePool()
        {
            CharmsPool = new List<CharmConfigBase>(_charmsPoolConfig.CharmsPool);

            foreach (var activeCharm in _charmsService.ActiveCharms)
                CharmsPool.Remove(activeCharm.Config);
        }
        
        public void Initialize()
        {
            _charmsService.OnCharmAdded += HandleCharmAdded;
            _charmsService.OnCharmRemoved += HandleCharmRemoved;
        }

        public void Dispose()
        {
            _charmsService.OnCharmAdded -= HandleCharmAdded;
            _charmsService.OnCharmRemoved -= HandleCharmRemoved;
        }

        private void HandleCharmAdded(CharmBase charm)
        {
            CharmsPool.Remove(charm.Config);
        }

        private void HandleCharmRemoved(CharmBase charm)
        {
            CharmsPool.Add(charm.Config);
        }
    }
}