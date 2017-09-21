using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class CandlesManager : ICandlesManager
    {
        private readonly IMidPriceQuoteGenerator _midPriceQuoteGenerator;
        private readonly IAssetPairsManager _assetPairsManager;
        private readonly ICandlesGenerator _candlesGenerator;
        private readonly ICandlesPublisher _publisher;

        public CandlesManager(
            IMidPriceQuoteGenerator midPriceQuoteGenerator,
            IAssetPairsManager assetPairsManager,
            ICandlesGenerator candlesGenerator,
            ICandlesPublisher publisher)
        {
            _midPriceQuoteGenerator = midPriceQuoteGenerator;
            _assetPairsManager = assetPairsManager;
            _candlesGenerator = candlesGenerator;
            _publisher = publisher;
        }

        public async Task ProcessQuoteAsync(IQuote quote)
        {
            var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(quote.AssetPair);

            if (assetPair == null)
            {
                return;
            }

            var midPriceQuote = _midPriceQuoteGenerator.TryGenerate(quote, assetPair.Accuracy);

            foreach (var timeInterval in Constants.PublishedIntervals)
            {
                var candleMergeResult = _candlesGenerator.Merge(quote, quote.IsBuy ? PriceType.Bid : PriceType.Ask, timeInterval);
                var tasks = new List<Task>();

                if (candleMergeResult.WasChanged)
                {
                    tasks.Add(_publisher.PublishAsync(candleMergeResult.Candle));
                }

                if (midPriceQuote != null)
                {
                    var midPriceCandleMergeResult = _candlesGenerator.Merge(midPriceQuote, PriceType.Mid, timeInterval);

                    if (midPriceCandleMergeResult.WasChanged)
                    {
                        tasks.Add(_publisher.PublishAsync(midPriceCandleMergeResult.Candle));
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}