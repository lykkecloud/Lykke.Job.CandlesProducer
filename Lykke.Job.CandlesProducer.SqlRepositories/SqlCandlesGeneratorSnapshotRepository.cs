using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Lykke.Job.CandlesProducer.AzureRepositories;
using Lykke.Job.CandlesProducer.Core.Domain;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.SqlRepositories.Extensions;
using Newtonsoft.Json;

namespace Lykke.Job.CandlesProducer.SqlRepositories
{
    public class SqlCandlesGeneratorSnapshotRepository : ISnapshotRepository<ImmutableDictionary<string, ICandle>>
    {
        private const string TableName = "CandlesGeneratorSnapshot";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[BlobKey] [nvarchar] (64) NOT NULL PRIMARY KEY, " +
                                                 "[Data] [nvarchar] (MAX) NULL, " +
                                                 "[Timestamp] [DateTime] NULL " +
                                                 ");";

        private readonly string _connectionString;

        public SqlCandlesGeneratorSnapshotRepository(string connectionString)
        {
            _connectionString = connectionString;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
            }
        }

        public async Task<ImmutableDictionary<string, ICandle>> TryGetAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var data = (await conn.QueryAsync<string>(
                    $"SELECT Data FROM {TableName} WHERE BlobKey=@blobKey",
                    new { blobKey = "CandlesGeneratorSnapshot" })).SingleOrDefault();

                if (string.IsNullOrEmpty(data))
                    return null;

                var model = JsonConvert.DeserializeObject<Dictionary<string, CandleEntity>>(data);

                return model.ToImmutableDictionary(i => i.Key, i => (ICandle)i.Value);
            }
        }

        public async Task SaveAsync(ImmutableDictionary<string, ICandle> state)
        {

            var model = state.ToDictionary(i => i.Key, i => CandleEntity.Copy(i.Value));

            var request = new
            {
                data = JsonConvert.SerializeObject(model),
                blobKey = "CandlesGeneratorSnapshot",
                timestamp = DateTime.Now
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
