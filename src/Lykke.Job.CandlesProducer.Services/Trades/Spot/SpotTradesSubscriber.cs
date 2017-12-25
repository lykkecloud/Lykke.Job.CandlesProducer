using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Job.CandlesProducer.Services.Trades.Spot.Messages;

namespace Lykke.Job.CandlesProducer.Services.Trades.Spot
{
    public class SpotTradesSubscriber : ITradesSubscriber
    {
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly IRabbitSubscriptionSettings _tradesSubscriptionSettings;
        private IStopable _marketTradesSubscriber;
        private IStopable _limitTradesSubscriber;

        public SpotTradesSubscriber(ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, IRabbitSubscriptionSettings tradesSubscriptionSettings)
        {
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _tradesSubscriptionSettings = tradesSubscriptionSettings;
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
                .Select(t => new Trade(
                    message.Order.AssetPairId,
                    message.Order.Volume > 0 ? TradeType.Buy : TradeType.Sell,
                    t.Timestamp,
                    message.Order.Volume > 0 ? t.LimitVolume : t.MarketVolume,
                    message.Order.Price ?? 0
                ));

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
                    .Select(t => new Trade(
                        o.Order.AssetPairId,
                        o.Order.Volume > 0 ? TradeType.Buy : TradeType.Sell,
                        t.Timestamp,
                        o.Order.Volume > 0 ? t.OppositeVolume : t.Volume,
                        o.Order.Price ?? 0
                    )));

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
