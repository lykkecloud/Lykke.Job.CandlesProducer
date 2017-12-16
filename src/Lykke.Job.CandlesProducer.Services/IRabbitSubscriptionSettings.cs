namespace Lykke.Job.CandlesProducer.Services
{
    public interface IRabbitSubscriptionSettings
    {
        string ConnectionString { get; }
        string EndpointName { get; }
    }
}
