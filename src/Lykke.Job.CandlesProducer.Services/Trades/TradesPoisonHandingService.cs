using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services;

namespace Lykke.Job.CandlesProducer.Services.Trades
{
    public class TradesPoisonHandingService<T> : ITradesPoisonHandingService where T : class
    {
        private IRabbitPoisonHandingService<T> _rabbitPoisonHandingService;

        public TradesPoisonHandingService(IRabbitPoisonHandingService<T> rabbitPoisonHandingService)
        {
            _rabbitPoisonHandingService = rabbitPoisonHandingService;
        }

        public async Task<string> PutTradesBack()
        {
            return await _rabbitPoisonHandingService.PutMessagesBack();
        }
    }
}
