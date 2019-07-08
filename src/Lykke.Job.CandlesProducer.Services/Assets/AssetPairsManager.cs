// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Job.CandlesProducer.Services.Assets
{
    public class AssetPairsManager : IAssetPairsManager
    {
        private readonly ICachedAssetsService _apiService;

        public AssetPairsManager(ICachedAssetsService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IAssetPair> TryGetEnabledPairAsync(string assetPairId)
        {
            var pair = await _apiService.TryGetAssetPairAsync(assetPairId);

            return pair == null || pair.IsDisabled ? null : pair;
        }
    }
}