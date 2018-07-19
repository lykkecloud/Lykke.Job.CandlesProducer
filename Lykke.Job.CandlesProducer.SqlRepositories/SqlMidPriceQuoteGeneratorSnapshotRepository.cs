using System;
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
using Microsoft.Extensions.Internal;

namespace Lykke.Job.CandlesProducer.SqlRepositories
{
    public class SqlMidPriceQuoteGeneratorSnapshotRepository : ISnapshotRepository<IImmutableDictionary<string, IMarketState>>
    {
        private const string TableName = "MidPriceQuoteGeneratorSnapshot";
        private const string BlobKey = "MidPriceQuoteGenerator";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[BlobKey] [nvarchar] (64) NOT NULL PRIMARY KEY, " +
                                                 "[Data] [nvarchar] (MAX) NULL, " +
                                                 "[Timestamp] [DateTime] NULL " +
                                                 ");";

        private readonly string _connectionString;
        private readonly ISystemClock _systemClock;

        public SqlMidPriceQuoteGeneratorSnapshotRepository( string connectionString)
        {
            _systemClock = new SystemClock();
            _connectionString = connectionString;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
            }
        }

        public async Task<IImmutableDictionary<string, IMarketState>> TryGetAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                 var data = (await conn.QueryAsync<string>(
                    $"SELECT Data FROM {TableName} WHERE BlobKey=@blobKey",
                    new {blobKey = BlobKey })).SingleOrDefault();

                if (string.IsNullOrEmpty(data))
                    return null;

                var model = JsonConvert.DeserializeObject<Dictionary<string, MarketStateEntity>>(data);

                return model.ToImmutableDictionary(i => i.Key, i => (IMarketState)i.Value);
            }
        }

        public async Task SaveAsync(IImmutableDictionary<string, IMarketState> state)
        {

                var model = state.ToDictionary(i => i.Key, i => MarketStateEntity.Create(i.Value));

                var request = new
                {
                    data = JsonConvert.SerializeObject(model),
                    blobKey = BlobKey,
                    timestamp = _systemClock.UtcNow.UtcDateTime
                };

                using (var conn = new SqlConnection(_connectionString))
                {
                    try
                    {
                        await conn.ExecuteAsync(
                            $"insert into {TableName} (BlobKey, Data, Timestamp) values (@blobKey, @data, @timestamp)",
                            request);
                    }
                    catch
                    {
                        await conn.ExecuteAsync(
                            $"update {TableName} set Data=@data, Timestamp = @timestamp where BlobKey=@blobKey",
                            request);
                    }
            }
           
        }
    }

}
