using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services;

namespace Lykke.Job.CandlesProducer.Services.Quotes
{
    public class QuotesPoisonHandingService<T> : IQuotesPoisonHandingService where T : class
    {
        private IRabbitPoisonHandingService<T> _rabbitPoisonHandingService;

        public QuotesPoisonHandingService(IRabbitPoisonHandingService<T> rabbitPoisonHandingService)
        {
            _rabbitPoisonHandingService = rabbitPoisonHandingService;
        }

        public async Task<string> PutQuotesBack()
        {
            return await _rabbitPoisonHandingService.PutMessagesBack();
        }
    }
}
