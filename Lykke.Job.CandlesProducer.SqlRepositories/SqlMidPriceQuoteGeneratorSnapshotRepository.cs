﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Lykke.Job.CandlesProducer.Contract;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using MessagePack;
using Newtonsoft.Json;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Logs.MsSql.Extensions;
using MarginTrading.SqlRepositories;
using Microsoft.Extensions.Internal;

namespace Lykke.Job.CandlesProducer.SqlRepositories
{
    public class SqlMidPriceQuoteGeneratorSnapshotRepository : ISnapshotRepository<IImmutableDictionary<string, IMarketState>>
    {
        private const string BlobContainer = "MidPriceQuoteGeneratorSnapshot";
        private const string Key = "MidPriceQuoteGenerator";
        private readonly ICandlesProducerBlobRepository _blobRepository;


        public SqlMidPriceQuoteGeneratorSnapshotRepository( string connectionString)
        {
            _blobRepository = new SqlBlobRepository(connectionString);
        }
    
        public async Task<IImmutableDictionary<string, IMarketState>> TryGetAsync()
        {
           return _blobRepository.Read<ImmutableDictionary<string, IMarketState>>(BlobContainer, Key);
        }

        public async Task SaveAsync(IImmutableDictionary<string, IMarketState> state)
        {
           await _blobRepository.Write(BlobContainer, Key, state);
        }
    }

}
