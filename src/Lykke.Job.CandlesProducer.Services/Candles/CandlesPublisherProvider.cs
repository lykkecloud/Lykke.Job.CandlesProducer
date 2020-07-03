// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisherProvider : ICandlesPublisherProvider
    {
        private readonly IEnumerable<ICandlesPublisher> _publishers;
        private readonly IDefaultCandlesPublisher _defaultPublisher;

        private readonly ConcurrentDictionary<string, ICandlesPublisher> _assetToPublisherMap =
            new ConcurrentDictionary<string, ICandlesPublisher>();

        public CandlesPublisherProvider(IEnumerable<ICandlesPublisher> publishers, IDefaultCandlesPublisher defaultPublisher)
        {
            _publishers = publishers ?? throw new ArgumentNullException(nameof(publishers));
            _defaultPublisher = defaultPublisher ?? throw new ArgumentNullException(nameof(defaultPublisher));
        }
            
        public ICandlesPublisher GetForAssetPair(string assetPair)
        {
            var publisher = _assetToPublisherMap.GetValueOrDefault(assetPair);

            if (publisher == null)
            {
                var matchedPublishers = _publishers
                    .Where(p => p.CanPublish(assetPair))
                    .ToList();

                publisher = matchedPublishers.Count == 1 ? matchedPublishers.Single() : _defaultPublisher;

                _assetToPublisherMap.TryAdd(assetPair, publisher);
            }

            return publisher;
        }
    }
}
