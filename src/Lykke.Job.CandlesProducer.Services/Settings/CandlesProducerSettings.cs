namespace Lykke.Job.CandlesProducer.Services.Settings
{
    public class CandlesProducerSettings
    {
        public DbSettings Db { get; set; }
        public AssetsCacheSettings AssetsCache { get; set; }
        public RabbitConnectionsStringSettings Rabbit { get; set; }
    }
}