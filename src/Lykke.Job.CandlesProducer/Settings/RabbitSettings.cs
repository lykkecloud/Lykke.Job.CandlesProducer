﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class RabbitSettings
    {
        [Optional]
        [AmqpCheck]
        public string QuotesSubscribtion { get; set; }
        public RabbitSubscriptionSettingsSettings TradesSubscription { get; set; }
        public CandlesPublicationRabbitSettings CandlesPublication { get; set; }
    }
}
