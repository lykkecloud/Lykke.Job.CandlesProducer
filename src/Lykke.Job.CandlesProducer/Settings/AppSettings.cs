// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {        
        [Optional]
        public CandlesProducerSettings CandlesProducerJob { get; set; }
        
        [Optional]
        public CandlesProducerSettings MtCandlesProducerJob { get; set; }
        
        [Optional, CanBeNull]
        public SlackNotificationsSettings SlackNotifications { get; set; }
        
        public AssetsSettings Assets { get; set; }
    }
}
