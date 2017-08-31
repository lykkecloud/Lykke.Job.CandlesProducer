using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Custom;

namespace Lykke.Job.CandlesProducer.Core.Services.Assets
{
    public interface IAssetPairsManager
    {
        Task<IAssetPair> TryGetEnabledPairAsync(string assetPairId);
    }
}