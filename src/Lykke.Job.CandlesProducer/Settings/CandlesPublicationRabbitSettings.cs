﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.Job.CandlesProducer.Services;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesPublicationRabbitSettings : IRabbitPublicationSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
    }
}

