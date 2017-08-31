using System;
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
            try
            {
                var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(quote.AssetPair);

                if (assetPair == null)
                {
                    return;
                }

                var midPriceQuote = _midPriceQuoteGenerator.TryGenerate(quote, assetPair.Accuracy);

                // TODO: Publish only changed candles
                foreach (var timeInterval in Constants.PublishedIntervals)
                {
                    var candle = _candlesGenerator.GenerateCandle(quote, timeInterval, quote.IsBuy ? PriceType.Bid : PriceType.Ask);
                    var tasks = new List<Task>
                    {
                        _publisher.PublishAsync(candle)
                    };
                    
                    if (midPriceQuote != null)
                    {
                        var midPriceCandle = _candlesGenerator.GenerateCandle(midPriceQuote, timeInterval, PriceType.Mid);

                        tasks.Add(_publisher.PublishAsync(midPriceCandle));
                    }

                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process quote: {quote.ToJson()}", ex);
            }
        }
    }
}