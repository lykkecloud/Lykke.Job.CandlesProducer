using Lykke.Job.CandlesProducer.Services;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesPublicationRabbitSettings : IRabbitPublicationSettings
    {
        public string ConnectionString { get; set; }
        public string Namespace { get; set; }
    }
}

