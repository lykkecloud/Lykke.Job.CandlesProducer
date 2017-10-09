using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        private Dictionary<string, MarketState> _assetMarketStates;

        public MidPriceQuoteGenerator()
        {
            _assetMarketStates = new Dictionary<string, MarketState>();
        }

        public IImmutableDictionary<string, IMarketState> GetState()
        {
            return _assetMarketStates.ToImmutableDictionary(i => i.Key, i => (IMarketState)i.Value);
        }

        public void SetState(IImmutableDictionary<string, IMarketState> state)
        {
            if (_assetMarketStates.Count > 0)
            {
                throw new InvalidOperationException("State already not empty");
            }

            _assetMarketStates = state.ToDictionary(i => i.Key, i => new MarketState(i.Value.Ask, i.Value.Bid));
        }

        public string DescribeState(IImmutableDictionary<string, IMarketState> state)
        {
            return $"Assets count: {state.Count}";
        }

        public IQuote TryGenerate(IQuote quote, int assetPairAccuracy)
        {
            var assetPairId = quote.AssetPair.Trim().ToUpper();

            _assetMarketStates.TryGetValue(assetPairId, out MarketState oldState);

            var newPriceState = new PriceState(quote.Price, quote.Timestamp);
            var newState = quote.IsBuy
                ? new MarketState(oldState?.Ask, newPriceState)
                : new MarketState(newPriceState, oldState?.Bid);

            _assetMarketStates[assetPairId] = newState;
            
            return TryCreateMidQuote(assetPairId, newState, assetPairAccuracy);
        }

        private static IQuote TryCreateMidQuote(string assetPairId, IMarketState marketState, int assetPairAccuracy)
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
