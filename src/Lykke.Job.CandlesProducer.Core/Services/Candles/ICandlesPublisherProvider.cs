// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface ICandlesPublisherProvider
    {
        Task<ICandlesPublisher> GetForAssetPair(string assetPair);
    }
}
