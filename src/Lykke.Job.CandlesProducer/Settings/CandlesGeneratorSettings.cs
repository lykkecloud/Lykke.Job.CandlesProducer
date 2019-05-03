using System;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.CandlesProducer.Settings
{
    public class CandlesGeneratorSettings
    {
        public TimeSpan OldDataWarningTimeout { get; set; }

        [Optional] public bool GenerateBidAndAsk { get; set; }
        
        [Optional] public bool GenerateTrades { get; set; }

        [Optional]
        public CandleTimeInterval[] TimeIntervals { get; set; } = new[]
        {
            CandleTimeInterval.Minute,
            CandleTimeInterval.Min5,
            CandleTimeInterval.Min15,
            CandleTimeInterval.Min30,
            CandleTimeInterval.Hour,
            CandleTimeInterval.Hour4,
            CandleTimeInterval.Day,
            CandleTimeInterval.Week,
            CandleTimeInterval.Month
        };
    }
}
