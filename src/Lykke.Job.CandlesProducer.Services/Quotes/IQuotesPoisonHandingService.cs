using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.CandlesProducer.Services.Quotes
{
    public interface IQuotesPoisonHandingService
    {
        Task<string> PutQuotesBack();
    }
}
