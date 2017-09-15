using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Domain.Prices;
using Lykke.Domain.Prices.Contracts;
using Lykke.Domain.Prices.Model;
using Lykke.Job.CandlesProducer.Core;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Settings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class QuotesSubscriber : IQuotesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly RabbitSettingsWithDeadLetter _rabbitSettings;

        private RabbitMqSubscriber<IQuote> _subscriber;

        public QuotesSubscriber(ILog log, ICandlesManager candlesManager, RabbitSettingsWithDeadLetter rabbitSettings)
        {
            _log = log;
            _candlesManager = candlesManager;
            _rabbitSettings = rabbitSettings;
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitSettings.ConnectionString,
                QueueName = $"{_rabbitSettings.ExchangeName}.candlesproducer",
                ExchangeName = _rabbitSettings.ExchangeName,
                DeadLetterExchangeName = _rabbitSettings.DeadLetterExchangeName,
                RoutingKey = "",
                IsDurable = true
            };

            try
            {
                _subscriber = new RabbitMqSubscriber<IQuote>(settings, 
                    new ResilientErrorHandlingStrategy(_log, settings, 
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<Quote>())
                    .SetMessageReadStrategy(new MessageReadQueueStrategy())
                    .Subscribe(ProcessQuoteAsync)
                    .CreateDefaultBinding()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(QuotesSubscriber), nameof(Start), null, ex).Wait();
                throw;
            }
        }

        public void Stop()
        {
            _subscriber.Stop();
        }

        private async Task ProcessQuoteAsync(IQuote quote)
        {
            try
            {
                var validationErrors = ValidateQuote(quote);
                if (validationErrors.Any())
                {
                    var message = string.Join("\r\n", validationErrors);
                    await _log.WriteWarningAsync(nameof(QuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), message);

                    return;
                }

                await _candlesManager.ProcessQuoteAsync(quote);
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(nameof(QuotesSubscriber), nameof(ProcessQuoteAsync), $"Failed to process quote: {quote.ToJson()}", ex);
            }
        }

        private static IReadOnlyCollection<string> ValidateQuote(IQuote quote)
        {
            var errors = new List<string>();

            if (quote == null)
            {
                errors.Add("Argument 'Order' is null.");
            }
            else
            {
                if (string.IsNullOrEmpty(quote.AssetPair))
                {
                    errors.Add("Empty 'AssetPair'");
                }
                if (quote.Timestamp.Kind != DateTimeKind.Utc)
                {
                    errors.Add($"Invalid 'Timestamp' Kind (UTC is required): '{quote.Timestamp.Kind}'");
                }
            }

            return errors;
        }

        public void Dispose()
        {
            _subscriber.Dispose();
        }
    }
}