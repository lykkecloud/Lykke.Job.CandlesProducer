﻿using System.Linq;
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
        private readonly string _connectionString;
        private IStopable _marketTradesSubscriber;
        private IStopable _limitTradesSubscriber;

        public SpotTradesSubscriber(ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, string connectionString)
        {
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _marketTradesSubscriber = _subscribersFactory.Create<MarketTradesMessage>(_connectionString, "lykke", "trades", ProcessMarketTradesAsync);
            _limitTradesSubscriber = _subscribersFactory.Create<LimitTradesMessage>(_connectionString, "lykke", "limitorders.clients", ProcessLimitTradesAsync);
        }

        private async Task ProcessMarketTradesAsync(MarketTradesMessage message)
        {
            if (message.Trades == null || !message.Trades.Any())
            {
                return;
            }

            var trades = message
                .Trades
                .Where(t => t.MarketVolume > 0)
                .Select(t => new Trade(
                    message.Order.AssetPairId,
                    t.Timestamp,
                    t.Price,
                    t.MarketVolume
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
                    .Where(t => t.Volume > 0)
                    .Select(t => new Trade(
                        o.Order.AssetPairId,
                        t.Timestamp,
                        t.Price,
                        t.Volume
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
