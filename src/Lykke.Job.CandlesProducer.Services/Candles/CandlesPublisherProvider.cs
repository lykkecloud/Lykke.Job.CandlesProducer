// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisherProvider : ICandlesPublisherProvider
    {
        private readonly IEnumerable<ICandlesPublisher> _publishers;
        private readonly IDefaultCandlesPublisher _defaultPublisher;
        
        public CandlesPublisherProvider(IEnumerable<ICandlesPublisher> publishers, IDefaultCandlesPublisher defaultPublisher)
        {
            _publishers = publishers ?? throw new ArgumentNullException(nameof(publishers));
            _defaultPublisher = defaultPublisher ?? throw new ArgumentNullException(nameof(defaultPublisher));
        }
            
        public ICandlesPublisher GetForAssetPair(string assetPair)
        {
            var matchedPublishers = _publishers
                .Where(p => p.CanPublish(assetPair))
                .ToList();

            if (matchedPublishers.Count == 1)
                return matchedPublishers.Single();
            
            return _defaultPublisher;
        }
    }
}
