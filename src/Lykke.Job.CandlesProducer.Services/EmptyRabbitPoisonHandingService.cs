using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Services;

namespace Lykke.Job.CandlesProducer.Services
{
    public class EmptyRabbitPoisonHandingService<T> : IRabbitPoisonHandingService<T> where T : class
    {
        public Task<string> PutMessagesBack()
        {
            return Task.FromResult(string.Empty);
        }
    }
}
