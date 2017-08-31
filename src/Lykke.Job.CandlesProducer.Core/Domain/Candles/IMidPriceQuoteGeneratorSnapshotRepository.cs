using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public interface IMidPriceQuoteGeneratorSnapshotRepository
    {
        Task SaveAsync(IEnumerable<KeyValuePair<string, IMarketState>> state);
        Task<IEnumerable<KeyValuePair<string, IMarketState>>> TryGetAsync();
    }
}