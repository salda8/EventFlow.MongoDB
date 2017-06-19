using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;
using EventFlow.MongoDB.ReadStores;
using EventFlow.MongoDB.Tests.IntegrationTests.ReadModels;

namespace EventFlow.MongoDB.Tests.IntegrationTests.QueryHandlers
{
    public class MongoDbThingyGetQueryHandler : IQueryHandler<ThingyGetQuery, Thingy>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IReadModelDescriptionProvider _readModelDescriptionProvider;

        public MongoDbThingyGetQueryHandler(
            IMongoDatabase mongoDatabase,
            IReadModelDescriptionProvider readModelDescriptionProvider)
        {
            _mongoDatabase = mongoDatabase;
            _readModelDescriptionProvider = readModelDescriptionProvider;
        }
        public Task<Thingy> ExecuteQueryAsync(ThingyGetQuery query, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<MongoDbThingyReadModel>();
            var collection = _mongoDatabase.GetCollection<MongoDbThingyReadModel>(readModelDescription.RootCollectionName.Value);
            var filter = Builders<MongoDbThingyReadModel>.Filter.Eq(readModel => readModel._id, query.ThingyId.Value);
            var result = collection.Find(filter).FirstOrDefault();

            return Task.FromResult(
                result == null ?
                null :
                result.ToThingy());
        }
    }
}
