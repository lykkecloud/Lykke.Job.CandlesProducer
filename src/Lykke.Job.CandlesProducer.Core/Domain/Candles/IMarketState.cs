namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public interface IMarketState
    {
        IPriceState Ask { get; }
        IPriceState Bid { get; }
    }
}