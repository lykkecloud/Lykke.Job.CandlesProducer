using System;
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

namespace Lykke.Job.CandlesProducer.Services.Quotes.Mt
{
    public class MtQuotesSubscriber : IQuotesSubscriber
    {
        private readonly ILog _log;
        private readonly ICandlesManager _candlesManager;
        private readonly IRabbitMqSubscribersFactory _subscribersFactory;
        private readonly string _connectionString;

        private IStopable _subscriber;

        public MtQuotesSubscriber(ILog log, ICandlesManager candlesManager, IRabbitMqSubscribersFactory subscribersFactory, string connectionString)
        {
            _log = log;
            _candlesManager = candlesManager;
            _subscribersFactory = subscribersFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            _subscriber = _subscribersFactory.Create<MtQuoteMessage>(_connectionString, "lykke.mt", "pricefeed", ProcessQuoteAsync);
        }

        public void Stop()
        {
            _subscriber?.Stop();
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
            _subscriber?.Dispose();
        }
    }
}
