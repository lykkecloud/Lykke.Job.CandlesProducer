// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesShardingSettings
    {
        [Optional]
        public IList<CandlesPublicationShard> Shards { get; set; }
        
        [Optional]
        public string DefaultShardName { get; set; }
    }
}
