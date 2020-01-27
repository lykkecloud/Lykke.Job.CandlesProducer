using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Services.Trades
{
    public interface ITradesPoisonHandingService
    {
        Task<string> PutTradesBack();
    }
}
