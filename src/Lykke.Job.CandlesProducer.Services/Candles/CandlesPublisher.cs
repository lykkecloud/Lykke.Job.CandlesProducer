using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Domain.Prices;
using Lykke.Job.CandlesProducer.Core;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker.Publisher;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisher : ICandlesPublisher
    {
        private class CandleMessage : ICandle
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

        private readonly ILog _log;
        private readonly AppSettings.RabbitSettings _rabbitSettings;

        private RabbitMqPublisher<ICandle> _publisher;

        public CandlesPublisher(ILog log, AppSettings.RabbitSettings rabbitSettings)
        {
            _log = log;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = new RabbitMqBroker.Subscriber.RabbitMqSubscriptionSettings()
            {
               ConnectionString = _rabbitSettings.ConnectionString,
               ExchangeName = _rabbitSettings.ExchangeName,
               IsDurable = true,
               RoutingKey = ""
            };

            _publisher = new RabbitMqPublisher<ICandle>(settings)
                .SetSerializer(new JsonMessageSerializer<ICandle>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .SetLogger(_log)
                .Start();
        }

        public Task PublishAsync(ICandle candle)
        {
            return _publisher.ProduceAsync(CandleMessage.Create(candle));
        }

        public void Dispose()
        {
            _publisher.Dispose();
        }

        public void Stop()
        {
            _publisher.Stop();
        }
    }
}