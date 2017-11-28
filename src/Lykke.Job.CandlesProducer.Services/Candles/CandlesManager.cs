using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.QuotesProducer.Contract;

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

        public async Task ProcessQuoteAsync(QuoteMessage quote)
        {
            var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(quote.AssetPair);

            if (assetPair == null)
            {
                return;
            }

            var midPriceQuote = _midPriceQuoteGenerator.TryGenerate(
                quote.AssetPair,
                quote.IsBuy,
                quote.Price, quote.Timestamp, assetPair.Accuracy);
            var changedUpdateResults = new ConcurrentBag<CandleUpdateResult>();
            var tasks = Constants
                .PublishedIntervals
                .Select(timeInterval => Task.Factory.StartNew(() =>
                {
                    ProcessInterval(
                        quote.AssetPair,
                        quote.Timestamp,
                        quote.Price,
                        0,
                        quote.IsBuy ? CandlePriceType.Bid : CandlePriceType.Ask,
                        timeInterval,
                        midPriceQuote,
                        changedUpdateResults);
                }));

            await ExecuteProcessingTasksAsync(tasks, changedUpdateResults);
        }

        public async Task ProcessTradeAsync(Trade trade)
        {
            var assetPair = await _assetPairsManager.TryGetEnabledPairAsync(trade.AssetPair);

            if (assetPair == null)
            {
                return;
            }

            var midPriceQuote = _midPriceQuoteGenerator.TryGenerate(
                trade.AssetPair,
                trade.Type == TradeType.Buy, 
                trade.Price, trade.Timestamp, assetPair.Accuracy);
            var changedUpdateResults = new ConcurrentBag<CandleUpdateResult>();
            var tasks = Constants
                .PublishedIntervals
                .Select(timeInterval => Task.Factory.StartNew(() =>
                {
                    ProcessInterval(
                        trade.AssetPair,
                        trade.Timestamp,
                        trade.Price,
                        trade.Volume,
                        trade.Type == TradeType.Buy ? CandlePriceType.Ask : CandlePriceType.Bid,
                        timeInterval,
                        midPriceQuote,
                        changedUpdateResults);
                }));

            await ExecuteProcessingTasksAsync(tasks, changedUpdateResults);
        }

        private async Task ExecuteProcessingTasksAsync(IEnumerable<Task> tasks, ConcurrentBag<CandleUpdateResult> changedUpdateResults)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // Failed to publish one or several candles, so processing should be cancelled

                foreach (var updateResult in changedUpdateResults)
                {
                    _candlesGenerator.Undo(updateResult);
                }

                throw;
            }
        }

        private void ProcessInterval(
            string assetPair,
            DateTime timestamp,
            double price,
            double tradingVolume,
            CandlePriceType priceType,
            CandleTimeInterval timeInterval,
            QuoteMessage midPriceQuote,
            ConcurrentBag<CandleUpdateResult> changedUpdateResults)
        {
            var candleUpdateResult = _candlesGenerator.Update(
                assetPair,
                timestamp,
                price,
                tradingVolume,
                priceType,
                timeInterval);

            if (candleUpdateResult.WasChanged)
            {
                changedUpdateResults.Add(candleUpdateResult);

                _publisher.PublishAsync(candleUpdateResult.Candle);
            }

            if (midPriceQuote != null)
            {
                var midPriceCandleUpdateResult = _candlesGenerator.Update(
                    midPriceQuote.AssetPair,
                    midPriceQuote.Timestamp,
                    midPriceQuote.Price,
                    tradingVolume,
                    CandlePriceType.Mid,
                    timeInterval);

                if (midPriceCandleUpdateResult.WasChanged)
                {
                    changedUpdateResults.Add(midPriceCandleUpdateResult);

                    _publisher.PublishAsync(midPriceCandleUpdateResult.Candle);
                }
            }
        }
    }
}
