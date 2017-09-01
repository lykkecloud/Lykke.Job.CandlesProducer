namespace Lykke.Job.CandlesProducer.Core.Domain.Candles
{
    public class CandleMergeResult
    {
        public ICandle Candle { get; }
        public bool WasChanged { get; }

        public CandleMergeResult(ICandle candle, bool wasChanged)
        {
            Candle = candle;
            WasChanged = wasChanged;
        }
    }
}