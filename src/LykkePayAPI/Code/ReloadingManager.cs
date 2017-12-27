using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.SettingsReader;

namespace LykkePay.API.Code
{
    public class ReloadingManager : IReloadingManager<Settings>
    {

        public ReloadingManager(Settings currentSettings)
        {
            CurrentValue = currentSettings;
        }
#pragma warning disable 1998
        public async Task<Settings> Reload()
        {
            return CurrentValue;
        }
#pragma warning restore 1998
        public bool HasLoaded => true;
        public Settings CurrentValue { get; }
    }
}
