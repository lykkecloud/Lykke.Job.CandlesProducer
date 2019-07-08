// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
