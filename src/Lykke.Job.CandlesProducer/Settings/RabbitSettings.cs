namespace Lykke.Job.CandlesProducer.Settings
{
    public class RabbitSettings
    {
        public string QuotesSubscribtion { get; set; }
        public RabbitSubscriptionSettingsSettings TradesSubscription { get; set; }
        public CandlesPublicationRabbitSettings CandlesPublication { get; set; }
    }
}
