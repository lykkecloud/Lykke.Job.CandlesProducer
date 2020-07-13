// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesPublisherProvider : ICandlesPublisherProvider
    {
        private readonly IEnumerable<ICandlesPublisher> _publishers;
        private readonly IDefaultCandlesPublisher _defaultPublisher;
        private readonly ILog _log;

        private readonly ConcurrentDictionary<string, ICandlesPublisher> _assetToPublisherMap =
            new ConcurrentDictionary<string, ICandlesPublisher>();

        public CandlesPublisherProvider(IEnumerable<ICandlesPublisher> publishers, IDefaultCandlesPublisher defaultPublisher, ILog log)
        {
            _publishers = publishers ?? throw new ArgumentNullException(nameof(publishers));
            _defaultPublisher = defaultPublisher ?? throw new ArgumentNullException(nameof(defaultPublisher));
            _log = log;
        }
            
        public async Task<ICandlesPublisher> GetForAssetPair(string assetPair)
        {
            var publisher = _assetToPublisherMap.GetValueOrDefault(assetPair);

            if (publisher == null)
            {
                var matchedPublishers = _publishers
                    .Where(p => p.CanPublish(assetPair))
                    .ToList();

                if (matchedPublishers.Count == 1)
                {
                    publisher = matchedPublishers.Single();
                }
                else
                {
                    var names = string.Join(",", matchedPublishers.Select(p => p.ShardName));

                    await _log.WriteWarningAsync(nameof(CandlesPublisherProvider),
                        nameof(GetForAssetPair),
                        new {assetPair}.ToJson(),
                        $"There are {(matchedPublishers.Count > 1 ? "more than one" : "zero")} candles publisher configured for asset [{assetPair}], Publisher configurations in conflict: {names}");

                    publisher = _defaultPublisher;
                }

                _assetToPublisherMap.TryAdd(assetPair, publisher);
            }

            return publisher;
        }
    }
}
