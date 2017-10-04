using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Settings;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly ILog _log;
        private readonly CandlesPublicationRabbitSettings _settings;


        private RabbitMqPublisher<ICandle> _publisher;

        public CandlesPublisher(ILog log, CandlesPublicationRabbitSettings settings)
        {
            _log = log;
            _settings = settings;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.Namespace, "candles")
                .MakeDurable()
                .DelayTheRecconectionForA(delay: TimeSpan.FromSeconds(20));

            _publisher = new RabbitMqPublisher<ICandle>(settings)
                .SetSerializer(new JsonMessageSerializer<ICandle>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
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