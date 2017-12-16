namespace Lykke.Job.CandlesProducer.Services
{
    public interface IRabbitPublicationSettings
    {
        string ConnectionString { get; }
        string Namespace { get; }
    }
}
