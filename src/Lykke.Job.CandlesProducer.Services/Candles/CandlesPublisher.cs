using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Domain.Prices;
using Lykke.Job.CandlesProducer.Core;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandleMessage : ICandle
    {
        [JsonProperty("a")]
        public string AssetPairId { get; set; }

        [JsonProperty("p")]
        public PriceType PriceType { get; set; }

        [JsonProperty("i")]
        public TimeInterval TimeInterval { get; set; }

        [JsonProperty("t")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("o")]
        public double Open { get; set; }

        [JsonProperty("c")]
        public double Close { get; set; }

        [JsonProperty("h")]
        public double High { get; set; }

        [JsonProperty("l")]
        public double Low { get; set; }

        public static CandleMessage Create(ICandle candle)
        {
            return new CandleMessage
            {
                AssetPairId = candle.AssetPairId,
                PriceType = candle.PriceType,
                TimeInterval = candle.TimeInterval,
                Timestamp = candle.Timestamp,
                Open = candle.Open,
                Close = candle.Close,
                Low = candle.Low,
                High = candle.High
            };
        }
    }

    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly ILog _log;
        private readonly AppSettings.RabbitSettings _rabbitSettings;
        private readonly IPublishingQueueRepository<ICandle> _publishingQueueRepository;

        private RabbitMqPublisher<ICandle> _publisher;

        public CandlesPublisher(ILog log, AppSettings.RabbitSettings rabbitSettings, IPublishingQueueRepository<ICandle> publishingQueueRepository)
        {
            _log = log;
            _rabbitSettings = rabbitSettings;
            _publishingQueueRepository = publishingQueueRepository;
        }

        public void Start()
        {
            //TODO: remove "lykke." from exchange name in CandlesProducerJob.CandlesPublication.ExchangeName
            var settings = RabbitMqSubscriptionSettings.CreateForPublisher(_rabbitSettings.ConnectionString, _rabbitSettings.ExchangeName)
                .MakeDurable()
                .DelayTheRecconectionForA(delay: TimeSpan.FromSeconds(20));

            _publisher = new RabbitMqPublisher<ICandle>(settings)
                .SetSerializer(new JsonMessageSerializer<ICandle>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .SetQueueRepository(_publishingQueueRepository)
                .SetLogger(_log)
                .Start();
        }

        public Task PublishAsync(ICandle candle)
        {
            return _publisher.ProduceAsync(CandleMessage.Create(candle));
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