using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage;
using AzureStorage.Blob;
using Common.Log;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Job.CandlesProducer.Core;
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
        private readonly AppSettings _settings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public JobModule(AppSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
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
                _settings.Assets,
                _settings.CandlesProducerJob.AssetsCache.ExpirationPeriod));

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

            builder.RegisterType<QuotesSubscriber>()
                .As<IQuotesSubscriber>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CandlesProducerJob.QuotesSubscribtion));

            builder.RegisterType<CandlesPublisher>()
                .As<ICandlesPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate()
                .WithParameter(TypedParameter.From(_settings.CandlesProducerJob.CandlesPublication));

            builder.RegisterType<MidPriceQuoteGenerator>()
                .As<IMidPriceQuoteGenerator>()
                .SingleInstance();

            builder.RegisterType<CandlesGenerator>()
                .As<ICandlesGenerator>();

            builder.RegisterType<CandlesManager>()
                .As<ICandlesManager>();

            builder.RegisterType<MidPriceQuoteGeneratorSnapshotRepository>()
                .As<IMidPriceQuoteGeneratorSnapshotRepository>()
                .WithParameter(TypedParameter.From<IBlobStorage>(
                    new AzureBlobStorage(_settings.CandlesProducerJob.Db.SnapshotsConnectionString)));

            builder.RegisterType<MidPriceQuoteGeneratorSnapshotSerializer>()
                .As<IMidPriceQuoteGeneratorSnapshotSerializer>();
        }
    }
}