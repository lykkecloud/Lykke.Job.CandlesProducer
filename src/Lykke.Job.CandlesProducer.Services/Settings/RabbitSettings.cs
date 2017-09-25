namespace Lykke.Job.CandlesProducer.Services.Settings
{
    public class RabbitSettings
    {
        public string QuotesSubscribtion { get; set; }
        public CandlesPublicationRabbitSettings CandlesPublication { get; set; }
    }
}