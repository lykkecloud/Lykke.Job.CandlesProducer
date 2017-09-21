using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisher : ICandlesPublisher
    {
        private readonly ILog _log;
        private readonly string _rabbitConnectionsString;
        private readonly IPublishingQueueRepository<ICandle> _publishingQueueRepository;

        private RabbitMqPublisher<ICandle> _publisher;

        public CandlesPublisher(ILog log, string rabbitConnectionsString, IPublishingQueueRepository<ICandle> publishingQueueRepository)
        {
            _log = log;
            _rabbitConnectionsString = rabbitConnectionsString;
            _publishingQueueRepository = publishingQueueRepository;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_rabbitConnectionsString, "candles")
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