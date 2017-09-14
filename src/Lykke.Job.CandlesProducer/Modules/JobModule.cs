using System.Collections.Immutable;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Blob;
using Common.Log;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Job.CandlesProducer.Core;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services;
using Lykke.Job.CandlesProducer.Services.Assets;
using Lykke.Job.CandlesProducer.Services.Candles;
using Lykke.Service.Assets.Client.Custom;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.CandlesProducer.Modules
{
    public class JobModule : Module
    {        
        private readonly ILog _log;
        private readonly IServiceCollection _services;
        private readonly AppSettings.CandlesProducerSettings _settings;
        private readonly QuotesSourceType _quotesSourceType;
        private readonly AssetsSettings _assetsSettings;        

        public JobModule(AppSettings.CandlesProducerSettings settings, QuotesSourceType quotesSourceType, AssetsSettings assetsSettings, ILog log)
        {
            _settings = settings;
            _quotesSourceType = quotesSourceType;
            _assetsSettings = assetsSettings;            
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {   
            builder.RegisterInstance(_settings.QuotesSubscribtion)
                .SingleInstance();
            builder.RegisterInstance(_settings.CandlesPublication)
                .SingleInstance();

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
                _assetsSettings,
                _settings.AssetsCache.ExpirationPeriod));

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


            builder.RegisterType(_quotesSourceType == QuotesSourceType.Spot
                    ? typeof(QuotesSubscriber)
                    : typeof(MtQuotesSubscriber))
                .As<IQuotesSubscriber>()
                .SingleInstance();

            builder.RegisterType<CandlesPublisher>()
                .As<ICandlesPublisher>()
                .SingleInstance();                

            builder.RegisterType<MidPriceQuoteGenerator>()
                .As<IMidPriceQuoteGenerator>()
                .As<IHaveState<IImmutableDictionary<string, IMarketState>>>()
                .SingleInstance();

            builder.RegisterType<CandlesGenerator>()
                .As<ICandlesGenerator>()
                .As<IHaveState<IImmutableDictionary<string, ICandle>>>()
                .SingleInstance();

            builder.RegisterType<CandlesManager>()
                .As<ICandlesManager>();

            builder.RegisterType<MidPriceQuoteGeneratorSnapshotRepository>()
                .As<ISnapshotRepository<IImmutableDictionary<string, IMarketState>>>()
                .WithParameter(TypedParameter.From<IBlobStorage>(
                    new AzureBlobStorage(_settings.Db.SnapshotsConnectionString)));

            builder.RegisterType<SnapshotSerializer<IImmutableDictionary<string, IMarketState>>>()
                .As<ISnapshotSerializer>();

            builder.RegisterType<CandlesGeneratorSnapshotRepository>()
                .As<ISnapshotRepository<IImmutableDictionary<string, ICandle>>>()
                .WithParameter(TypedParameter.From<IBlobStorage>(
                    new AzureBlobStorage(_settings.Db.SnapshotsConnectionString)));

            builder.RegisterType<SnapshotSerializer<IImmutableDictionary<string, ICandle>>>()
                .As<ISnapshotSerializer>()
                .PreserveExistingDefaults();
        }
    }
}