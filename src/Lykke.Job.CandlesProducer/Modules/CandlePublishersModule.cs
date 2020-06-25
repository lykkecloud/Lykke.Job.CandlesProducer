// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services;
using Lykke.Job.CandlesProducer.Services.Candles;
using Lykke.Job.CandlesProducer.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.CandlesProducer.Modules
{
    public class CandlePublishersModule : Module
    {
        private readonly CandlesPublicationRabbitSettings _candlesPublicationRabbitSettings;
        private readonly IServiceCollection _services;
        
        private const string DefaultShardName = "default";

        public CandlePublishersModule(CandlesPublicationRabbitSettings candlesPublicationRabbitSettings)
        {
            _candlesPublicationRabbitSettings = candlesPublicationRabbitSettings;
            _services = new ServiceCollection();
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMqPublishersFactory>()
                .As<IRabbitMqPublishersFactory>();
            
            var defaultShardName =
                _candlesPublicationRabbitSettings.CandlesSharding?.DefaultShardName ?? DefaultShardName;

            builder.RegisterType<DefaultCandlesPublisher>()
                .As<IDefaultCandlesPublisher>()
                .SingleInstance()
                .WithParameter("connectionString", _candlesPublicationRabbitSettings.ConnectionString)
                .WithParameter("nspace", _candlesPublicationRabbitSettings.Namespace)
                .WithParameter("shardName", defaultShardName);

            var shards = _candlesPublicationRabbitSettings.CandlesSharding?.Shards ??
                         Enumerable.Empty<CandlesPublicationShard>();
            
            foreach (var shard in shards)
            {
                if (shard.Name == defaultShardName)
                    throw new InvalidOperationException(
                        $"The shard name [{defaultShardName}] can't be used since it is already in use by default candles publisher. Please rename.");

                builder.RegisterType<CandlesPublisher>()
                    .As<ICandlesPublisher>()
                    .SingleInstance()
                    .WithParameter("connectionString", _candlesPublicationRabbitSettings.ConnectionString)
                    .WithParameter("nspace", _candlesPublicationRabbitSettings.Namespace)
                    .WithParameter("shardName", shard.Name)
                    .WithParameter("shardPattern", shard.Pattern);
            }

            builder.RegisterType<CandlesPublisherProvider>()
                .As<ICandlesPublisherProvider>()
                .SingleInstance();

            builder.Populate(_services);
        }
    }
}
