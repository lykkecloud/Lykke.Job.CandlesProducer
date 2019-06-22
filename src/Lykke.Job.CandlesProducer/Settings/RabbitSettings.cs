using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class RabbitSettings
    {
        [Optional]
        [AmqpCheck]
        public string QuotesSubscribtion { get; set; }

        [Optional] public int QuoteConsumersCount { get; set; } = 1;
        public RabbitSubscriptionSettingsSettings TradesSubscription { get; set; }
        public CandlesPublicationRabbitSettings CandlesPublication { get; set; }
    }
}
