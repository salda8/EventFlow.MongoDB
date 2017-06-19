using EventFlow.Queries;
using EventFlow.TestHelpers.Aggregates.Entities;
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
    public class MongoDbThingyGetMessagesQueryHandler : IQueryHandler<ThingyGetMessagesQuery, IReadOnlyCollection<ThingyMessage>>
    {
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IReadModelDescriptionProvider _readModelDescriptionProvider;

        public MongoDbThingyGetMessagesQueryHandler(
            IMongoDatabase mongoDatabase,
            IReadModelDescriptionProvider readModelDescriptionProvider)
        {
            _mongoDatabase = mongoDatabase;
            _readModelDescriptionProvider = readModelDescriptionProvider;
        }
        public Task<IReadOnlyCollection<ThingyMessage>> ExecuteQueryAsync(ThingyGetMessagesQuery query, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<MongoDbThingyMessageReadmodel>();
            var collection = _mongoDatabase.GetCollection<MongoDbThingyMessageReadmodel>(readModelDescription.RootCollectionName.Value);
            var filter = Builders<MongoDbThingyMessageReadmodel>.Filter.Eq(readModel => readModel.ThingyId, query.ThingyId.Value);
            var result = collection.Find(filter).ToList();
            var list = (IReadOnlyCollection<ThingyMessage>)result.Select(s => s.ToThingyMessage()).ToList();

            return Task.FromResult(list);
        }
    }
}
