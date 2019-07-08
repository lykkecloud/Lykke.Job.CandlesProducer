// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;

namespace Lykke.Job.CandlesProducer.Core.Services
{
    public interface IRabbitMqSubscribersFactory
    {
        IStopable Create<TMessage>(string connectionString, string @namespace, string source, Func<TMessage, Task> handler, string queueSuffix = null);
    }
}
