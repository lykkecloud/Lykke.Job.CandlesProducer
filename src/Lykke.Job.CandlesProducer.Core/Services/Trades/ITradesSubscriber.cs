using Autofac;
using Common;

namespace Lykke.Job.CandlesProducer.Core.Services.Trades
{
    public interface ITradesSubscriber : IStartable, IStopable
    {
    }
}
