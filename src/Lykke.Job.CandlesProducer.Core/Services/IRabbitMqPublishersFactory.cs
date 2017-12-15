using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitMqPublishersFactory
    {
        RabbitMqPublisher<TMessage> Create<TMessage>(IRabbitMqSerializer<TMessage> serializer, string connectionString, string @namespace, string endpoint);
    }
}
