namespace Lykke.Job.CandlesProducer.Services.Settings
{
    public class RabbitSettingsWithDeadLetter : RabbitSettings
    {
        public string DeadLetterExchangeName { get; set; }
    }
}