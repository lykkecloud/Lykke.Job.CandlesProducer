using System;
using System.Reflection;
using Lykke.HttpClientGenerator.Caching;

namespace Lykke.Job.CandlesProducer.CachingStrategies
{
    public class PlainTimeoutCachingStrategy : ICachingStrategy
    {
        private TimeSpan Timeout { get; }
        
        public PlainTimeoutCachingStrategy(TimeSpan timeout)
        {
            Timeout = timeout;
        }

        public TimeSpan GetCachingTime(MethodInfo targetMethod, object[] args)
        {
            return Timeout;
        }
    }
}
