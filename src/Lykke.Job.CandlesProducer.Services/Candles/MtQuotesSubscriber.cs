using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common;
using Common.Log;
using Lykke.Domain.Prices.Model;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    public class MtQuotesSubscriber : IQuotesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly string _rabbitConnectionString;

        private RabbitMqSubscriber<MtQuote> _subscriber;

        public MtQuotesSubscriber(ILog log, ICandlesManager candlesManager, string rabbitConnectionString)
        {
            _log = log;
            _candlesManager = candlesManager;
            _rabbitConnectionString = rabbitConnectionString;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_rabbitConnectionString, "lykke.mt", "pricefeed", "lykke.mt", "candlesproducer")
                .MakeDurable();

            try
            {
                _subscriber = new RabbitMqSubscriber<MtQuote>(settings, 
                    new ResilientErrorHandlingStrategy(_log, settings, 
                        retryTimeout: TimeSpan.FromSeconds(5),
                        retryNum: int.MaxValue,
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<MtQuote>())
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

        private async Task ProcessQuoteAsync(MtQuote quote)
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

                var bidQuote = new Quote
                {
                    AssetPair = quote.Instrument,
                    IsBuy = true,
                    Price = quote.Bid,
                    Timestamp = quote.Date
                };

                var askQuote = new Quote
                {
                    AssetPair = quote.Instrument,
                    IsBuy = false,
                    Price = quote.Ask,
                    Timestamp = quote.Date
                };

                await _candlesManager.ProcessQuoteAsync(bidQuote);
                await _candlesManager.ProcessQuoteAsync(askQuote);
            }
            catch (Exception)
            {
                await _log.WriteWarningAsync(nameof(MtQuotesSubscriber), nameof(ProcessQuoteAsync), quote.ToJson(), "Failed to process quote");
                throw;
            }
        }

        private static IReadOnlyCollection<string> ValidateQuote(MtQuote quote)
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
            _subscriber.Dispose();
        }
    }
}
