namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesProducerSettings
    {
        public DbSettings Db { get; set; }
        public AssetsCacheSettings AssetsCache { get; set; }
        public RabbitSettings Rabbit { get; set; }
        public CandlesGeneratorSettings CandlesGenerator { get; set; }
    }
}
