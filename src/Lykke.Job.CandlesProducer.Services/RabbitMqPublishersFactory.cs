// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services
{
    [UsedImplicitly]
    public class RabbitMqPublishersFactory : IRabbitMqPublishersFactory
    {
        private readonly ILog _log;

        public RabbitMqPublishersFactory(ILog log)
        {
            _log = log;
        }

        public RabbitMqPublisher<TMessage> Create<TMessage>(
            IRabbitMqSerializer<TMessage> serializer, 
            string connectionString, 
            string @namespace, 
            string endpoint)
        {
            try
            {
                var settings = RabbitMqSubscriptionSettings
                    .CreateForPublisher(connectionString, @namespace, endpoint)
                    .MakeDurable();

                return new RabbitMqPublisher<TMessage>(settings)
                    .SetSerializer(serializer)
                    .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                    .PublishSynchronously()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(RabbitMqPublishersFactory), nameof(Create), null, ex).Wait();
                throw;
            }
        }
    }
}
