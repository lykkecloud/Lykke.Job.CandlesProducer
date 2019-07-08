// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Lykke.Job.CandlesProducer.Services
{
    public interface IRabbitSubscriptionSettings
    {
        string ConnectionString { get; }
        string EndpointName { get; }
    }
}
