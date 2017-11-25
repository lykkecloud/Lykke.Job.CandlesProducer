using System;
using System.Collections.Immutable;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using Common.Log;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Quotes;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Job.CandlesProducer.Services;
using Lykke.Job.CandlesProducer.Services.Assets;
using Lykke.Job.CandlesProducer.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Quotes.Mt;
using Lykke.Job.CandlesProducer.Services.Quotes.Spot;
using Lykke.Job.CandlesProducer.Services.Settings;
using Lykke.Job.CandlesProducer.Services.Trades.Mt;
using Lykke.Job.CandlesProducer.Services.Trades.Spot;
using Lykke.Service.Assets.Client.Custom;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.CandlesProducer.Modules
{
    // TODO: Register MT trades subscriber

    public class JobModule : Module
    {
        private readonly IReloadingManager<CandlesProducerSettings> _settings;
        private readonly IReloadingManager<AssetsSettings> _assetsSettings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;
        private readonly QuotesSourceType _quotesSourceType;

        public JobModule(IReloadingManager<CandlesProducerSettings> settings, IReloadingManager<AssetsSettings> assetsSettings, QuotesSourceType quotesSourceType, ILog log)
        {
            _settings = settings;
            _assetsSettings = assetsSettings;
            _quotesSourceType = quotesSourceType;
            _log = log;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            RegisterAssetsServices(builder);

            RegisterCandlesServices(builder);

            builder.Populate(_services);
        }

        private void RegisterAssetsServices(ContainerBuilder builder)
        {
            _services.UseAssetsClient(AssetServiceSettings.Create(
                _assetsSettings.CurrentValue,
                _settings.CurrentValue.AssetsCache.ExpirationPeriod));

            builder.RegisterType<AssetPairsManager>()
                .As<IAssetPairsManager>()
                .SingleInstance();
        }

        private void RegisterCandlesServices(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<RabbitMqSubscribersFactory>()
                .As<IRabbitMqSubscribersFactory>();

            builder.RegisterType(_quotesSourceType == QuotesSourceType.Spot
                    ? typeof(SpotQuotesSubscriber)
                    : typeof(MtQuotesSubscriber))
                .As<IQuotesSubscriber>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Rabbit.QuotesSubscribtion));

            builder.RegisterType(_quotesSourceType == QuotesSourceType.Spot
                    ? typeof(SpotTradesSubscriber)
                    : typeof(MtTradesSubscriber))
                .As<ITradesSubscriber>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Rabbit.TradesSubscription));

            builder.RegisterType<CandlesPublisher>()
                .As<ICandlesPublisher>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.Rabbit.CandlesPublication));

            builder.RegisterType<MidPriceQuoteGenerator>()
                .As<IMidPriceQuoteGenerator>()
                .As<IHaveState<IImmutableDictionary<string, IMarketState>>>()
                .SingleInstance();

            builder.RegisterType<CandlesGenerator>()
                .As<ICandlesGenerator>()
                .As<IHaveState<ImmutableDictionary<string, ImmutableList<ICandle>>>>()
                .SingleInstance();

            builder.RegisterType<CandlesManager>()
                .As<ICandlesManager>();

            var snapshotsConnStringManager = _settings.Nested(x => x.Db.SnapshotsConnectionString);

            builder.RegisterType<MidPriceQuoteGeneratorSnapshotRepository>()
                .As<ISnapshotRepository<IImmutableDictionary<string, IMarketState>>>()
                .WithParameter(TypedParameter.From(AzureBlobStorage.Create(snapshotsConnStringManager, maxExecutionTimeout: TimeSpan.FromMinutes(5))));

            builder.RegisterType<SnapshotSerializer<IImmutableDictionary<string, IMarketState>>>()
                .As<ISnapshotSerializer>();

            builder.RegisterType<CandlesGeneratorSnapshotRepository>()
                .As<ISnapshotRepository<ImmutableDictionary<string, ImmutableList<ICandle>>>>()
                .WithParameter(TypedParameter.From(AzureBlobStorage.Create(snapshotsConnStringManager, maxExecutionTimeout: TimeSpan.FromMinutes(5))));

            builder.RegisterType<SnapshotSerializer<ImmutableDictionary<string, ImmutableList<ICandle>>>>()
                .As<ISnapshotSerializer>()
                .PreserveExistingDefaults();
        }
    }
}
