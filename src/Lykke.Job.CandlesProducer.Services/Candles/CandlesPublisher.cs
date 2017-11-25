using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Settings;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly ILog _log;
        private readonly CandlesPublicationRabbitSettings _settings;

        private RabbitMqPublisher<CandleMessage> _publisher;

        public CandlesPublisher(ILog log, CandlesPublicationRabbitSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.Namespace, "candles")
                .MakeDurable();

            _publisher = new RabbitMqPublisher<CandleMessage>(settings)
                .SetSerializer(new JsonMessageSerializer<CandleMessage>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .SetLogger(_log)
                .Start();
        }

        public Task PublishAsync(ICandle candle)
        {
            return _publisher.ProduceAsync(new CandleMessage
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
                LastUpdateTimestamp = candle.LastUpdateTimestamp
            });
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
