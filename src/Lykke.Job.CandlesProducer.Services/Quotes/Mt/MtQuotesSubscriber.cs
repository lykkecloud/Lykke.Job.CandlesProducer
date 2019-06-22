using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.CandlesProducer.Core.Services;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Quotes;
using Lykke.Job.CandlesProducer.Services.Quotes.Mt.Messages;
using Lykke.Job.QuotesProducer.Contract;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Quotes.Mt
{
    public class MtQuotesSubscriber : IQuotesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly string _connectionString;
        private readonly int _quoteConsumersCount;

        private readonly ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable> _subscribers =
            new ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable>(new SubscriptionSettingsWithNumberEqualityComparer());

        public MtQuotesSubscriber(ILog log, ICandlesManager candlesManager, 
            IRabbitMqSubscribersFactory subscribersFactory, string connectionString, int quoteConsumersCount)
        {
            _log = log;
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
            _quoteConsumersCount = quoteConsumersCount;
        }

        public void Start()
        {
            var consumerCount = _quoteConsumersCount == 0 ? 1 : _quoteConsumersCount;

            Subscribe(
                _connectionString, consumerCount, true, ProcessQuoteAsync,
                GetJsonDeserializer<MtQuoteMessage>());
        }

        public void Stop()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Value?.Stop();
            }
        }

        public IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>()
        {
            return new DeserializerWithErrorLogging<TMessage>(_log);
        }
        
        public void Subscribe<TMessage>(string connectionString, int quoteConsumersCount, bool isDurable,
            Func<TMessage, Task> handler, IMessageDeserializer<TMessage> deserializer)
        {
            var consumerCount = quoteConsumersCount == 0 ? 1 : quoteConsumersCount;
            
            foreach (var consumerNumber in Enumerable.Range(1, consumerCount))
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = connectionString,
                    QueueName = $"lykke.mt.pricefeed.candlesproducer",
                    ExchangeName = $"lykke.mt.pricefeed",
                    IsDurable = isDurable,
                };

                var rabbitMqSubscriber = new RabbitMqSubscriber<TMessage>(subscriptionSettings,
                        new DefaultErrorHandlingStrategy(_log, subscriptionSettings))
                    .SetMessageDeserializer(deserializer)
                    .Subscribe(handler)
                    .SetLogger(_log);
//                    .SetConsole(new ConsoleWriter());

                if (!_subscribers.TryAdd((subscriptionSettings, consumerNumber), rabbitMqSubscriber))
                {
                    throw new InvalidOperationException(
                        $"A subscriber number {consumerNumber} for queue {subscriptionSettings.QueueName} was already initialized");
                }

                rabbitMqSubscriber.Start();
            }
        }

        private async Task ProcessQuoteAsync(MtQuoteMessage quote)
        {
            try
            {
                var validationErrors = ValidateQuote(quote);
                if (validationErrors.Any())
                {
                    var message = string.Join("\r\n", validationErrors);
                    await _log.WriteWarningAsync(nameof(MtQuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), message);

                    return;
                }

                if (quote.Bid > 0)
                {
                    var bidQuote = new QuoteMessage
                    {
                        AssetPair = quote.Instrument,
                        IsBuy = true,
                        Price = quote.Bid,
                        Timestamp = quote.Date
                    };

                    await _candlesManager.ProcessQuoteAsync(bidQuote);
                }
                else
                {
                    await _log.WriteWarningAsync(nameof(MtQuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), "bid quote is skipped due to not positive price");
                }

                if (quote.Ask > 0)
                {
                    var askQuote = new QuoteMessage
                    {
                        AssetPair = quote.Instrument,
                        IsBuy = false,
                        Price = quote.Ask,
                        Timestamp = quote.Date
                    };

                    await _candlesManager.ProcessQuoteAsync(askQuote);
                }
                else
                {
                    await _log.WriteWarningAsync(nameof(MtQuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), "bid quote is skipped due to not positive price");
                }
            }
            catch (Exception)
            {
                await _log.WriteWarningAsync(nameof(MtQuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), "Failed to process quote");
                throw;
            }
        }

        private static IReadOnlyCollection<string> ValidateQuote(MtQuoteMessage quote)
        {
            var errors = new List<string>();

            if (quote == null)
            {
                errors.Add("Argument 'Order' is null.");
            }
            else
            {
                if (string.IsNullOrEmpty(quote.Instrument))
                {
                    errors.Add("Empty 'Instrument'");
                }
                if (quote.Date.Kind != DateTimeKind.Utc)
                {
                    errors.Add($"Invalid 'Date' Kind (UTC is required): '{quote.Date.Kind}'");
                }
            }

            return errors;
        }

        public void Dispose()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Value?.Stop();
            }
        }

        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class SubscriptionSettingsWithNumberEqualityComparer : IEqualityComparer<(RabbitMqSubscriptionSettings, int)>
        {
            public bool Equals((RabbitMqSubscriptionSettings, int) x, (RabbitMqSubscriptionSettings, int) y)
            {
                if (ReferenceEquals(x.Item1, y.Item1) && x.Item2 == y.Item2) return true;
                if (ReferenceEquals(x.Item1, null)) return false;
                if (ReferenceEquals(y.Item1, null)) return false;
                if (x.Item1.GetType() != y.Item1.GetType()) return false;
                return string.Equals(x.Item1.ConnectionString, y.Item1.ConnectionString)
                       && string.Equals(x.Item1.ExchangeName, y.Item1.ExchangeName)
                       && x.Item2 == y.Item2;
            }

            public int GetHashCode((RabbitMqSubscriptionSettings, int) obj)
            {
                unchecked
                {
                    return ((obj.Item1.ConnectionString != null ? obj.Item1.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.Item1.ExchangeName != null ? obj.Item1.ExchangeName.GetHashCode() : 0);
                }
            }
        }
    }
}
