// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.Job.CandlesProducer.Core.Services.Quotes;

namespace Lykke.Job.CandlesProducer.Services.Quotes
{
    public class EmptyQuotesSubscriber : IQuotesSubscriber
    {
        public void Start()
        {
            // Just do nothing.
        }

        public void Dispose()
        {
            // Just do nothing.
        }

        public void Stop()
        {
            // Just do nothing.
        }
    }
}
