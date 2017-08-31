using Autofac;
using Common;

namespace Lykke.Job.CandlesProducer.Core.Services.Candles
{
    public interface IQuotesSubscriber : IStartable, IStopable
    {
    }
}