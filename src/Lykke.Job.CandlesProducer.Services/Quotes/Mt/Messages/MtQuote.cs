using System;

namespace Lykke.Job.CandlesProducer.Services.Quotes.Mt.Messages
{
    public class MtQuote
    {
        public string Instrument { get; set; }
        public DateTime Date { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
    }
}
