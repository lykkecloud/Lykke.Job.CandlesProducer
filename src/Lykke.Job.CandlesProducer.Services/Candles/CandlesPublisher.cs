using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Candles.LegacyContract;
using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly IRabbitMqPublishersFactory _publishersFactory;
        private readonly IRabbitPublicationSettings _settings;

        private RabbitMqPublisher<CandleMessageV1> _legacyPublisher;
        private RabbitMqPublisher<CandlesUpdatedEvent> _publisher;

        public CandlesPublisher(IRabbitMqPublishersFactory publishersFactory, IRabbitPublicationSettings settings)
        {
            _publishersFactory = publishersFactory;
            _settings = settings;
        }

        public void Start()
        {
            _legacyPublisher = _publishersFactory.Create(
                new JsonMessageSerializer<CandleMessageV1>(),
                _settings.ConnectionString,
                _settings.Namespace,
                "candles");

            _publisher = _publishersFactory.Create(
                new MessagePackMessageSerializer<CandlesUpdatedEvent>(),
                _settings.ConnectionString,
                _settings.Namespace,
                "candles-v2");
        }

        public Task PublishAsync(IReadOnlyCollection<CandleUpdateResult> updates)
        {
            return Task.WhenAll(PublishV2Async(updates), PublishV1Async(updates));
        }

        private Task PublishV1Async(IEnumerable<CandleUpdateResult> updates)
        {
            lock (_legacyPublisher)
            {
                foreach (var candle in updates.Select(c => c.Candle))
                {
                    // HACK: Actually ProduceAsync is not async, so not need to await it and lock works well

                    _legacyPublisher.ProduceAsync(new CandleMessageV1
                    {
                        AssetPairId = candle.AssetPairId,
                        PriceType = candle.PriceType,
                        TimeInterval = candle.TimeInterval,
                        Timestamp = candle.Timestamp,
                        Open = candle.Open,
                        Close = candle.Close,
                        Low = candle.Low,
                        High = candle.High,
                        TradingVolume = candle.TradingVolume,
                        LastUpdateTimestamp = candle.LatestChangeTimestamp
                    });
                }
            }

            return Task.CompletedTask;
        }

        private Task PublishV2Async(IEnumerable<CandleUpdateResult> updates)
        {
            lock (_publisher)
            {
                // HACK: Actually ProduceAsync is not async, so lock works well

                return _publisher.ProduceAsync(new CandlesUpdatedEvent
                {
                    ContractVersion = Contract.Constants.ContractVersion,
                    UpdateTimestamp = DateTime.UtcNow,
                    Candles = updates
                        .Select(c => new CandleUpdate
                        {
                            IsLatestCandle = c.IsLatestCandle,
                            IsLatestChange = c.IsLatestChange,
                            ChangeTimestamp = c.Candle.LatestChangeTimestamp,
                            AssetPairId = c.Candle.AssetPairId,
                            PriceType = c.Candle.PriceType,
                            TimeInterval = c.Candle.TimeInterval,
                            CandleTimestamp = c.Candle.Timestamp,
                            Open = c.Candle.Open,
                            Close = c.Candle.Close,
                            Low = c.Candle.Low,
                            High = c.Candle.High,
                            TradingVolume = c.Candle.TradingVolume
                        })
                        .ToArray()
                });
            }
        }

        public void Dispose()
        {
            _legacyPublisher?.Dispose();
            _publisher?.Dispose();
        }

        public void Stop()
        {
            _legacyPublisher?.Stop();
            _publisher?.Stop();
        }
    }
}
