using System.Collections.Generic;
using System.Linq;
using VContainer;

namespace _Scripts.Charms
{
    public class CharmsPoolService
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private readonly CharmsPoolConfig _charmsPoolConfig;
        private readonly CharmsService _charmsService;

        [Inject]
        public CharmsPoolService(CharmsPoolConfig charmsPoolConfig, CharmsService charmsService)
        {
            _charmsPoolConfig = charmsPoolConfig;
            _charmsService = charmsService;
        }
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        public List<CharmConfigBase> GetAvailableCharms()
        {
            var activeConfigs = _charmsService.ActiveCharms
                .Select(c => c.Config)
                .ToHashSet();

            return _charmsPoolConfig.CharmsPool
                .Where(cfg => !activeConfigs.Contains(cfg))
                .ToList();
        }
    }
}