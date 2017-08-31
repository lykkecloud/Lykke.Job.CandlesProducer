using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly ILog _log;
        private readonly AppSettings.RabbitSettings _rabbitSettings;

        private RabbitMqPublisher<Candle> _publisher;

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

            _publisher = new RabbitMqPublisher<Candle>(settings)
                .SetSerializer(new JsonMessageSerializer<Candle>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .SetLogger(_log)
                .Start();
        }

        public Task PublishAsync(Candle candle)
        {
            return _publisher.ProduceAsync(candle);
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