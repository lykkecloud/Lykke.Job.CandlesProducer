using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Core.Settings
{
    [UsedImplicitly]
    public class AssetsCacheSettings
    {
        [Optional]
        public TimeSpan ExpirationPeriod { get; set; } = TimeSpan.FromMinutes(5);
    }
}
