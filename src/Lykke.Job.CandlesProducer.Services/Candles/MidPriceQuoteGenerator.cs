using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Lykke.Domain.Prices.Contracts;
using Lykke.Domain.Prices.Model;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class MidPriceQuoteGenerator : IMidPriceQuoteGenerator
    {
        private class PriceState : IPriceState
        {
            public double Price { get; }
            public DateTime Moment { get; }

            public PriceState(double price, DateTime moment)
            {
                Price = price;
                Moment = moment;
            }
        }

        private class MarketState : IMarketState
        {
            public IPriceState Ask { get; }
            public IPriceState Bid { get; }

            public MarketState(IPriceState ask, IPriceState bid)
            {
                Ask = ask;
                Bid = bid;
            }
        }

        private ConcurrentDictionary<string, MarketState> _assetMarketStates;

        public MidPriceQuoteGenerator()
        {
            _assetMarketStates = new ConcurrentDictionary<string, MarketState>();
        }

        public IEnumerable<KeyValuePair<string, IMarketState>> GetState()
        {
            return _assetMarketStates
                .ToArray()
                .Select(i => KeyValuePair.Create<string, IMarketState>(i.Key, i.Value));
        }

        public void SetState(IEnumerable<KeyValuePair<string, IMarketState>> state)
        {
            if (_assetMarketStates.Count > 0)
            {
                throw new InvalidOperationException("State already not empty");
            }

            _assetMarketStates = new ConcurrentDictionary<string, MarketState>(state
                .Select(i => KeyValuePair.Create(
                    i.Key,
                    new MarketState(i.Value.Ask, i.Value.Bid))));
        }

        public IQuote TryGenerate(IQuote quote, int assetPairAccuracy)
        {
            var assetPairId = quote.AssetPair.Trim().ToUpper();
            var state = _assetMarketStates.AddOrUpdate(
                assetPairId,
                k => AddNewAssetState(new PriceState(quote.Price, quote.Timestamp), quote.IsBuy),
                (k, oldState) => UpdateAssetState(oldState, new PriceState(quote.Price, quote.Timestamp), quote.IsBuy));

            return TryCreateMidQuote(assetPairId, state, assetPairAccuracy);
        }

        private static MarketState AddNewAssetState(PriceState priceState, bool isBid)
        {
            return isBid ? new MarketState(null, priceState) : new MarketState(priceState, null);
        }

        private static MarketState UpdateAssetState(MarketState oldState, PriceState priceState, bool isBid)
        {
            return isBid ? new MarketState(oldState.Ask, priceState) : new MarketState(priceState, oldState.Bid);
        }

        private static IQuote TryCreateMidQuote(string assetPairId, MarketState marketState, int assetPairAccuracy)
        {
            if (marketState.Bid != null && marketState.Ask != null)
            {
                return new Quote
                {
                    AssetPair = assetPairId,
                    Price = Math.Round((marketState.Ask.Price + marketState.Bid.Price) / 2, assetPairAccuracy),
                    Timestamp = marketState.Ask.Moment >= marketState.Bid.Moment
                        ? marketState.Ask.Moment
                        : marketState.Bid.Moment
                };
            }

            return null;
        }
    }
}