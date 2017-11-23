using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using JetBrains.Annotations;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
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

            foreach (var timeInterval in Constants.PublishedIntervalsHistoryDepth.Keys)
            {
                var candleUpdateResult = _candlesGenerator.Update(
                    quote.AssetPair,
                    quote.Timestamp,
                    quote.Price,
                    0,
                    quote.IsBuy ? PriceType.Bid : PriceType.Ask, 
                    timeInterval);

                var tasks = new List<Task>();

                if (candleUpdateResult.WasChanged)
                {
                    tasks.Add(_publisher.PublishAsync(candleUpdateResult.Candle));
                }

                if (midPriceQuote != null)
                {
                    var midPriceCandleUpdateResult = _candlesGenerator.Update(
                        midPriceQuote.AssetPair,
                        midPriceQuote.Timestamp,
                        midPriceQuote.Price,
                        0,
                        PriceType.Mid, 
                        timeInterval);

                    if (midPriceCandleUpdateResult.WasChanged)
                    {
                        tasks.Add(_publisher.PublishAsync(midPriceCandleUpdateResult.Candle));
                    }
                }

                await Task.WhenAll(tasks);
            }
        }

        public async Task ProcessTradeAsync(Trade trade)
        {
            var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(trade.AssetPair);

            if (assetPair == null)
            {
                return;
            }

            foreach (var timeInterval in Constants.PublishedIntervalsHistoryDepth.Keys)
            {
                var candleUpdateResult = _candlesGenerator.Update(
                    trade.AssetPair,
                    trade.Timestamp,
                    trade.Price,
                    trade.Volume,
                    trade.Type == TradeType.Buy ? PriceType.Ask : PriceType.Bid,
                    timeInterval);

                if (candleUpdateResult.WasChanged)
                {
                    try
                    {
                        await _publisher.PublishAsync(candleUpdateResult.Candle);
                    }
                    catch (Exception)
                    {
                        // Failed to publish the candle, so trade processing should be cancelled
                        // to avoid volume duplication when the trade will be processed again when
                        // subscriber will retry the message

                        _candlesGenerator.Undo(candleUpdateResult);
                    }
                }
            }
        }
    }
}
