using System.Linq;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Assets;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Job.CandlesProducer.Services.Trades.Spot.Messages;

namespace Lykke.Job.CandlesProducer.Services.Trades.Spot
{
    [UsedImplicitly]
    public class SpotTradesSubscriber : ITradesSubscriber
    {
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly IRabbitSubscriptionSettings _tradesSubscriptionSettings;
        private readonly IAssetPairsManager _assetPairsManager;
        private IStopable _marketTradesSubscriber;
        private IStopable _limitTradesSubscriber;

        public SpotTradesSubscriber(
            ICandlesManager candlesManager, 
            IRabbitMqSubscribersFactory subscribersFactory, 
            IRabbitSubscriptionSettings tradesSubscriptionSettings,
            IAssetPairsManager assetPairsManager)
        {
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _tradesSubscriptionSettings = tradesSubscriptionSettings;
            _assetPairsManager = assetPairsManager;
        }

        public void Start()
        {
            _marketTradesSubscriber = _subscribersFactory.Create<MarketTradesMessage>(_tradesSubscriptionSettings.ConnectionString, "lykke", _tradesSubscriptionSettings.EndpointName, ProcessMarketTradesAsync);
            _limitTradesSubscriber = _subscribersFactory.Create<LimitTradesMessage>(_tradesSubscriptionSettings.ConnectionString, "lykke", "limitorders.clients", ProcessLimitTradesAsync);
        }

        private async Task ProcessMarketTradesAsync(MarketTradesMessage message)
        {
            if (message.Trades == null || !message.Trades.Any())
            {
                return;
            }

            var trades = message
                .Trades
                .Select(t =>
                {
                    TradeType tradeType;
                    // Volumes in the asset pair base and quoting assets
                    double baseVolume;
                    double quotingVolume;

                    var assetPair = _assetPairsManager.TryGetEnabledPairAsync(message.Order.AssetPairId)
                        .GetAwaiter()
                        .GetResult();

                    if (t.MarketAsset == assetPair.BaseAssetId)
                    {
                        tradeType = message.Order.Volume > 0 && message.Order.Straight ||
                                    message.Order.Volume < 0 && !message.Order.Straight
                            ? TradeType.Buy
                            : TradeType.Sell;

                        baseVolume = t.MarketVolume;
                        quotingVolume = t.LimitVolume;
                    }
                    else
                    {
                        tradeType = message.Order.Volume > 0 && message.Order.Straight ||
                                    message.Order.Volume < 0 && !message.Order.Straight
                            ? TradeType.Sell
                            : TradeType.Buy;

                        baseVolume = t.LimitVolume;
                        quotingVolume = t.MarketVolume;
                    }

                    return new Trade(
                        message.Order.AssetPairId,
                        tradeType,
                        t.Timestamp,
                        baseVolume,
                        quotingVolume,
                        t.Price
                    );
                });

            foreach (var trade in trades)
            {
                await _candlesManager.ProcessTradeAsync(trade);
            }
        }

        private async Task ProcessLimitTradesAsync(LimitTradesMessage message)
        {
            if (message.Orders == null || !message.Orders.Any())
            {
                return;
            }

            var trades = message.Orders
                .SelectMany(o => o.Trades
                    .Select(t =>
                    {
                        var assetPair = _assetPairsManager.TryGetEnabledPairAsync(o.Order.AssetPairId)
                            .GetAwaiter()
                            .GetResult();

                        var tradeType = o.Order.Volume > 0 ? TradeType.Buy : TradeType.Sell;
                        // Volumes in the asset pair base and quoting assets
                        double baseVolume;
                        double quotingVolume;

                        if (t.Asset == assetPair.BaseAssetId)
                        {
                            baseVolume = t.Volume;
                            quotingVolume = t.OppositeVolume;
                        }
                        else
                        {
                            baseVolume = t.OppositeVolume;
                            quotingVolume = t.Volume;
                        }

                        return new Trade(
                            o.Order.AssetPairId,
                            tradeType,
                            t.Timestamp,
                            baseVolume,
                            quotingVolume,
                            t.Price
                        );
                    }));

            foreach (var trade in trades)
            {
                await _candlesManager.ProcessTradeAsync(trade);
            }
        }
        
        public void Stop()
        {
            _marketTradesSubscriber?.Stop();
            _limitTradesSubscriber?.Stop();
        }

        public void Dispose()
        {
            _marketTradesSubscriber?.Dispose();
            _limitTradesSubscriber?.Dispose();
        }
    }
}
