// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.RabbitMqBroker.Publisher;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitMqPublishersFactory
    {
        RabbitMqPublisher<TMessage> Create<TMessage>(IRabbitMqSerializer<TMessage> serializer, string connectionString, string @namespace, string endpoint);
    }
}
