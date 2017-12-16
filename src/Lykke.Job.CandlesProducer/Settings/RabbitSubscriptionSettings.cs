
using Lykke.Job.CandlesProducer.Services;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class RabbitSubscriptionSettingsSettings : IRabbitSubscriptionSettings
    {
        public string ConnectionString { get; set; }
        [Optional]
        public string EndpointName { get; set; }
    }
}
