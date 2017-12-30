﻿using System.Threading.Tasks;
using Common;
using Lykke.Job.CandlesProducer.Core.Domain.Trades;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Trades;
using Lykke.Job.CandlesProducer.Services.Trades.Mt.Messages;

namespace Lykke.Job.CandlesProducer.Services.Trades.Mt
{
    public class MtTradesSubscriber : ITradesSubscriber
    {
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly string _connectionString;
        private IStopable _tradesSubscriber;

        public MtTradesSubscriber(ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, string connectionString)
        {
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _tradesSubscriber = _subscribersFactory.Create<MtTradeMessage>(_connectionString, "lykke.mt", "trades", ProcessTradeAsync);
        }

        private async Task ProcessTradeAsync(MtTradeMessage message)
        {
            var quotingVolume = (double) (message.Volume * message.Price);

            var trade = new Trade(
                message.AssetPairId,
                message.Type == MtTradeMessage.TradeType.Buy ? TradeType.Buy : TradeType.Sell,
                message.Date,
                (double) message.Volume,
                quotingVolume,
                (double) message.Price);

            var oppositeTrade = new Trade(
                message.AssetPairId,
                message.Type == MtTradeMessage.TradeType.Buy ? TradeType.Sell : TradeType.Buy,
                message.Date,
                (double) message.Volume,
                quotingVolume,
                (double) message.Price);

            await Task.WhenAll(
                _candlesManager.ProcessTradeAsync(trade),
                _candlesManager.ProcessTradeAsync(oppositeTrade));
        }

        public void Dispose()
        {
            _tradesSubscriber?.Dispose();
        }

        public void Stop()
        {
            _tradesSubscriber?.Stop();
        }
    }
}
