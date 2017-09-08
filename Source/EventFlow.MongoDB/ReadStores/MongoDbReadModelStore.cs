using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.Extensions;
using EventFlow.Logs;
using EventFlow.ReadStores;
using MongoDB.Driver;
using EventFlow.MongoDB.ValueObjects;

namespace EventFlow.MongoDB.ReadStores
{
	


	public class MongoDbReadModelStore<TReadModel> : IMongoDbReadModelStore<TReadModel>
        where TReadModel : class, IMongoDbReadModel, new()
    {
        private readonly ILog _log;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IReadModelDescriptionProvider _readModelDescriptionProvider;

		public MongoDbReadModelStore(
            ILog log,
            IMongoDatabase mongoDatabase,
            IReadModelDescriptionProvider readModelDescriptionProvider)
        {
            _log = log;
            _mongoDatabase = mongoDatabase;
            _readModelDescriptionProvider = readModelDescriptionProvider;
		}

        public Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Information($"Deleting ALL '{typeof(TReadModel).PrettyPrint()}' by DROPPING COLLECTION '{readModelDescription.RootCollectionName}'!");

            _mongoDatabase.DropCollection(readModelDescription.RootCollectionName.Value, cancellationToken);

            return Task.FromResult(0);
        }

        public async Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Verbose(() => $"Fetching read model '{typeof(TReadModel).PrettyPrint()}' with _id '{id}' from collection '{readModelDescription.RootCollectionName}'");

            var collection = _mongoDatabase.GetCollection<TReadModel>(readModelDescription.RootCollectionName.Value);
            var filter = Builders<TReadModel>.Filter.Eq(readModel => readModel._id, id);
            var result = await collection.Find(filter).FirstAsync(cancellationToken);
            return ReadModelEnvelope<TReadModel>.With(id, result);
        }

	    public async Task<IAsyncCursor<TReadModel>> FindAsync(Expression<Func<TReadModel, bool>> filter, FindOptions<TReadModel, TReadModel> options = null, CancellationToken cancellationToken = new CancellationToken())
	    {
			var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();
		    var collection = _mongoDatabase.GetCollection<TReadModel>(readModelDescription.RootCollectionName.Value);

		    _log.Verbose(() => $"Finding read model '{typeof(TReadModel).PrettyPrint()}' with expression '{filter}' from collection '{readModelDescription.RootCollectionName}'");
			
			return await collection.FindAsync(filter, options, cancellationToken);
		}

		public async Task UpdateAsync(IReadOnlyCollection<ReadModelUpdate> readModelUpdates, IReadModelContext readModelContext, Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken, Task<ReadModelEnvelope<TReadModel>>> updateReadModel, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Verbose(() =>
            {
                var readModelIds = readModelUpdates
                    .Select(u => u.ReadModelId)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();
                return $"Updating read models of type '{typeof(TReadModel).PrettyPrint()}' with _ids '{string.Join(", ", readModelIds)}' in collection '{readModelDescription.RootCollectionName}'";
            });

            foreach (var readModelUpdate in readModelUpdates)
            {
                var collection = _mongoDatabase.GetCollection<TReadModel>(readModelDescription.RootCollectionName.Value);
                var filter = Builders<TReadModel>.Filter.Eq(readmodel => readmodel._id, readModelUpdate.ReadModelId);
                var result = collection.Find(filter).FirstOrDefault();

                var readModelEnvelope = result != null
                    ? ReadModelEnvelope<TReadModel>.With(readModelUpdate.ReadModelId, result)
                    : ReadModelEnvelope<TReadModel>.Empty(readModelUpdate.ReadModelId);

                readModelEnvelope = await updateReadModel(readModelContext, readModelUpdate.DomainEvents, readModelEnvelope, cancellationToken).ConfigureAwait(false);

                readModelEnvelope.ReadModel._version = readModelEnvelope.Version;

                await collection.ReplaceOneAsync<TReadModel>(
                    x => x._id == readModelUpdate.ReadModelId,
                    readModelEnvelope.ReadModel,
                    new UpdateOptions() { IsUpsert = true },
                    cancellationToken);
            }
        }
    }
}