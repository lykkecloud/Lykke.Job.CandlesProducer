﻿using System;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Job.CandlesProducer.Core
{
    public class AppSettings
    {
        public CandlesProducerSettings CandlesProducerJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsSettings Assets { get; set; }

        public class CandlesProducerSettings
        {
            public DbSettings Db { get; set; }
            public AssetsCacheSettings AssetsCache { get; set; }
            public RabbitConnectionStringSettings Rabbit { get; set; }
        }

        public class AssetsCacheSettings
        {
            public TimeSpan ExpirationPeriod { get; set; }
        }

        public class DbSettings
        {
            public string LogsConnString { get; set; }
            public string SnapshotsConnectionString { get; set; }
        }

        public class SlackNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }
        }

        public class AzureQueueSettings
        {
            public string ConnectionString { get; set; }

            public string QueueName { get; set; }
        }

        public class RabbitConnectionStringSettings
        {
            public string QuotesSubscribtion { get; set; }
            public string CandlesPublication { get; set; }
        }
    }
}