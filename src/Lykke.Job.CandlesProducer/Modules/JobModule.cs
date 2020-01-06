﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using Common.Log;
using Lykke.Common;
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
using Lykke.Job.CandlesProducer.Services.Quotes;
using Lykke.Job.CandlesProducer.Services.Quotes.Mt;
using Lykke.Job.CandlesProducer.Services.Quotes.Spot;
using Lykke.Job.CandlesProducer.Services.Trades.Mt;
using Lykke.Job.CandlesProducer.Services.Trades.Spot;
using Lykke.Job.CandlesProducer.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using MarginTrading.SettingsService.Contracts;
using Lykke.HttpClientGenerator;
using Lykke.Job.CandlesProducer.SqlRepositories;
using Lykke.Service.Assets.Client.Custom;
using AssetsSettings = Lykke.Job.CandlesProducer.Settings.AssetsSettings;
using Lykke.Job.CandlesProducer.Services.Trades.Spot.Messages;
using Lykke.Job.CandlesProducer.Services.Trades.Mt.Messages;
using Lykke.Job.QuotesProducer.Contract;
using Lykke.Job.CandlesProducer.Services.Quotes.Mt.Messages;
using Lykke.Job.CandlesProducer.Services.Trades;

namespace Lykke.Job.CandlesProducer.Modules
{
    public class JobModule : Module
    {
        private readonly CandlesProducerSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettings;
        private readonly AssetsSettings _assetsSettings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;
        private readonly QuotesSourceType _quotesSourceType;

        public JobModule(CandlesProducerSettings settings, IReloadingManager<DbSettings> dbSettings,
            AssetsSettings assetsSettings, QuotesSourceType quotesSourceType, ILog log)
        {
            _settings = settings;
            _dbSettings = dbSettings;
            _assetsSettings = assetsSettings;
            _quotesSourceType = quotesSourceType;
            _log = log;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log).As<ILog>().SingleInstance();

            builder.RegisterType<HealthService>().As<IHealthService>().SingleInstance();

            RegisterResourceMonitor(builder);

            RegisterAssetsServices(builder);

            RegisterCandlesServices(builder);

            builder.Populate(_services);
        }

        private void RegisterResourceMonitor(ContainerBuilder builder)
        {
            var monitorSettings = _settings.ResourceMonitor;

            if (monitorSettings != null)
                switch (monitorSettings.MonitorMode)
                {
                    case ResourceMonitorMode.Off:
                        // Do not register any resource monitor.
                        break;

                    case ResourceMonitorMode.AppInsightsOnly:
                        builder.RegisterResourcesMonitoring(_log);
                        break;

                    case ResourceMonitorMode.AppInsightsWithLog:
                        builder.RegisterResourcesMonitoringWithLogging(
                            _log,
                            monitorSettings.CpuThreshold,
                            monitorSettings.RamThreshold);
                        break;
                }
        }

        private void RegisterAssetsServices(ContainerBuilder builder)
        {
            if (_quotesSourceType == QuotesSourceType.Spot)
            {
                _services.UseAssetsClient(AssetServiceSettings.Create(
                    new Uri(_assetsSettings.ServiceUrl),
                _settings.AssetsCache.ExpirationPeriod));

                builder.RegisterType<AssetPairsManager>()
                        .As<IAssetPairsManager>()
                        .SingleInstance();
            }
            else
            {
                builder.RegisterClient<IAssetPairsApi>(_assetsSettings.ServiceUrl, builderConfigure =>
                    {
                        if (!string.IsNullOrWhiteSpace(_assetsSettings.ApiKey))
                        {
                            builderConfigure = builderConfigure.WithApiKey(_assetsSettings.ApiKey);
                        }

                        return builderConfigure;
                    });

                builder.RegisterType<MtAssetPairsManager>()
                    .AsSelf()
                    .As<IAssetPairsManager>()
                    .SingleInstance()
                    .OnActivated(args => args.Instance.Start())
                    .WithParameter(new TypedParameter(typeof(TimeSpan), _settings.AssetsCache.ExpirationPeriod));
            }
        }

        private void RegisterCandlesServices(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterType<RabbitMqSubscribersFactory>()
                .As<IRabbitMqSubscribersFactory>();

            builder.RegisterType<RabbitMqPublishersFactory>()
                .As<IRabbitMqPublishersFactory>();

            // Optionally loading quotes subscriber if it is present in settings...
            if (_settings.Rabbit.QuotesSubscribtion != null)
            {
                if (_quotesSourceType == QuotesSourceType.Spot)
                {
                    _services.AddSingleton<IRabbitPoisonHandingService<QuoteMessage>>(provider => new RabbitPoisonHandingService<QuoteMessage>(
                        provider.GetService<ILog>(),
                        provider.GetService<IQuotesSubscriber>().SubscriptionSettings));

                    _services.AddSingleton<IQuotesPoisonHandingService>(provider => new QuotesPoisonHandingService<QuoteMessage>(
                        provider.GetService<IRabbitPoisonHandingService<QuoteMessage>>()));

                    builder.RegisterType<SpotQuotesSubscriber>()
                        .As<IQuotesSubscriber>()
                        .SingleInstance()
                        .WithParameter(TypedParameter.From(_settings.Rabbit.QuotesSubscribtion))
                        .WithParameter(TypedParameter.From(_settings.SkipEodQuote));
                }
                else
                {
                    _services.AddSingleton<IRabbitPoisonHandingService<MtQuoteMessage>>(provider => new RabbitPoisonHandingService<MtQuoteMessage>(
                        provider.GetService<ILog>(),
                        provider.GetService<IQuotesSubscriber>().SubscriptionSettings));

                    _services.AddSingleton<IQuotesPoisonHandingService>(provider => new QuotesPoisonHandingService<MtQuoteMessage>(
                        provider.GetService<IRabbitPoisonHandingService<MtQuoteMessage>>()));

                    builder.RegisterType<MtQuotesSubscriber>()
                        .As<IQuotesSubscriber>()
                        .SingleInstance()
                        .WithParameter(TypedParameter.From(_settings.Rabbit.QuotesSubscribtion))
                        .WithParameter(TypedParameter.From(_settings.SkipEodQuote));
                }
            }
            else
            {
                _services.AddSingleton<IRabbitPoisonHandingService<EmptyQuote>>(provider => new RabbitPoisonHandingService<EmptyQuote>(
                       provider.GetService<ILog>(),
                       provider.GetService<IQuotesSubscriber>().SubscriptionSettings));

                _services.AddSingleton<IQuotesPoisonHandingService>(provider => new QuotesPoisonHandingService<EmptyQuote>(
                    provider.GetService<IRabbitPoisonHandingService<EmptyQuote>>()));

                builder.RegisterType<EmptyQuotesSubscriber>()
                    .As<IQuotesSubscriber>()
                    .SingleInstance();
            }

            if (_quotesSourceType == QuotesSourceType.Spot)
            {
                _services.AddSingleton<IRabbitPoisonHandingService<LimitOrdersMessage>>(provider => new RabbitPoisonHandingService<LimitOrdersMessage>(
                    provider.GetService<ILog>(),
                    provider.GetService<ITradesSubscriber>().SubscriptionSettings));

                _services.AddSingleton<ITradesPoisonHandingService>(provider => new TradesPoisonHandingService<LimitOrdersMessage>(
                    provider.GetService<IRabbitPoisonHandingService<LimitOrdersMessage>>()));

                builder.RegisterType<SpotTradesSubscriber>()
                    .As<ITradesSubscriber>()
                    .SingleInstance()
                    .WithParameter(TypedParameter.From<IRabbitSubscriptionSettings>(_settings.Rabbit.TradesSubscription));
            }
            else
            {
                _services.AddSingleton<IRabbitPoisonHandingService<MtTradeMessage>>(provider => new RabbitPoisonHandingService<MtTradeMessage>(
                    provider.GetService<ILog>(),
                    provider.GetService<ITradesSubscriber>().SubscriptionSettings));

                _services.AddSingleton<ITradesPoisonHandingService>(provider => new TradesPoisonHandingService<MtTradeMessage>(
                    provider.GetService<IRabbitPoisonHandingService<MtTradeMessage>>()));

                builder.RegisterType<MtTradesSubscriber>()
                    .As<ITradesSubscriber>()
                    .SingleInstance()
                    .WithParameter(TypedParameter.From(_settings.Rabbit.TradesSubscription.ConnectionString))
                    .WithParameter(TypedParameter.From(_settings.CandlesGenerator.GenerateTrades));
            }

            builder.RegisterType<CandlesPublisher>()
                .As<ICandlesPublisher>()
                .SingleInstance()
                .WithParameter(TypedParameter.From<IRabbitPublicationSettings>(_settings.Rabbit.CandlesPublication));

            builder.RegisterType<MidPriceQuoteGenerator>()
                .As<IMidPriceQuoteGenerator>()
                .As<IHaveState<IImmutableDictionary<string, IMarketState>>>()
                .SingleInstance();

            builder.RegisterType<CandlesGenerator>()
                .As<ICandlesGenerator>()
                .As<IHaveState<ImmutableDictionary<string, ICandle>>>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.CandlesGenerator.OldDataWarningTimeout));

            builder.RegisterType<CandlesManager>()
                .As<ICandlesManager>()
                .WithParameter(TypedParameter.From(_settings.CandlesGenerator.TimeIntervals))
                .WithParameter(TypedParameter.From(_settings.CandlesGenerator.GenerateBidAndAsk));

            if (_settings.Db.StorageMode == StorageMode.SqlServer)
            {
                var connstrParameter = new NamedParameter("connectionString",
                    _settings.Db.SnapshotsConnectionString);

                builder.Register<ISnapshotRepository<IImmutableDictionary<string, IMarketState>>>(ctx =>
                        new SqlMidPriceQuoteGeneratorSnapshotRepository(_settings.Db.SnapshotsConnectionString))
                    .SingleInstance();


                builder.RegisterType<SnapshotSerializer<IImmutableDictionary<string, IMarketState>>>()
                    .As<ISnapshotSerializer>();

                builder.Register<ISnapshotRepository<ImmutableDictionary<string, ICandle>>>(ctx =>
                        new SqlCandlesGeneratorSnapshotRepository(_settings.Db.SnapshotsConnectionString))
                    .SingleInstance();


                builder.RegisterType<SnapshotSerializer<ImmutableDictionary<string, ICandle>>>()
                    .As<ISnapshotSerializer>();

            }
            else if (_settings.Db.StorageMode == StorageMode.Azure)
            {
                var snapshotsConnStringManager = _dbSettings.ConnectionString(x => x.SnapshotsConnectionString);

                builder.RegisterType<MidPriceQuoteGeneratorSnapshotRepository>()
                    .As<ISnapshotRepository<IImmutableDictionary<string, IMarketState>>>()
                    .WithParameter(TypedParameter.From(AzureBlobStorage.Create(snapshotsConnStringManager, maxExecutionTimeout: TimeSpan.FromMinutes(5))));

                builder.RegisterType<SnapshotSerializer<IImmutableDictionary<string, IMarketState>>>()
                    .As<ISnapshotSerializer>();

                builder.RegisterType<CandlesGeneratorSnapshotRepository>()
                    .As<ISnapshotRepository<ImmutableDictionary<string, ICandle>>>()
                    .WithParameter(TypedParameter.From(AzureBlobStorage.Create(snapshotsConnStringManager, maxExecutionTimeout: TimeSpan.FromMinutes(5))))
                    .SingleInstance();

                builder.RegisterType<SnapshotSerializer<ImmutableDictionary<string, ICandle>>>()
                    .As<ISnapshotSerializer>()
                    .PreserveExistingDefaults();
            }


        }
    }
}
