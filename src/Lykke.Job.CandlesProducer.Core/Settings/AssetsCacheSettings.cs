using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Core.Settings
{
    [UsedImplicitly]
    public class AssetsCacheSettings
    {
        public TimeSpan ExpirationPeriod { get; set; }

        [Optional] 
        public int AssetPairsRefreshPeriodMs { get; set; } = 600000;
    }
}
