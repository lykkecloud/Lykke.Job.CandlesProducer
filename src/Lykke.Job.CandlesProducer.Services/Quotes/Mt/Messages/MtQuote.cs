// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.Services.Quotes.Mt.Messages
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class MtQuoteMessage
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Instrument { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DateTime Date { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public double Bid { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public double Ask { get; set; }
        
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsEod { get; set; }
    }
}
