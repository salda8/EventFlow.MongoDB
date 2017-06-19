using EventFlow.Aggregates;
using EventFlow.Extensions;
using EventFlow.Logs;
using EventFlow.ReadStores;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventFlow.MongoDB.ReadStores
{
    public class MongoDbInsertOnlyReadModelStore<TReadModel> : IMongoDbInsertOnlyReadModelStore<TReadModel>
        where TReadModel : class, IMongoDbInsertOnlyReadModel, new()
    {
        private readonly ILog _log;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IInsertOnlyReadModelDescriptionProvider _readModelDescriptionProvider;

        public MongoDbInsertOnlyReadModelStore(
            ILog log,
            IMongoDatabase mongoDatabase,
            IInsertOnlyReadModelDescriptionProvider readModelDescriptionProvider)
        {
            _log = log;
            _mongoDatabase = mongoDatabase;
            _readModelDescriptionProvider = readModelDescriptionProvider;
        }

        public Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Information($"Deleting ALL '{typeof(TReadModel).PrettyPrint()}' by DROPPING COLLECTION '{readModelDescription.RootCollectionName}'!");

            _mongoDatabase.DropCollection(readModelDescription.RootCollectionName.Value);

            return Task.FromResult(0);
        }

        public async Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            return ReadModelEnvelope<TReadModel>.Empty(id);
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
                var readModelEnvelope = ReadModelEnvelope<TReadModel>.Empty(readModelUpdate.ReadModelId);

                readModelEnvelope = await updateReadModel(readModelContext, readModelUpdate.DomainEvents, readModelEnvelope, cancellationToken).ConfigureAwait(false);
                readModelEnvelope.ReadModel._id = ObjectIdGenerator.Instance.GenerateId(collection, readModelEnvelope.ReadModel);
                await collection.InsertOneAsync(
                    readModelEnvelope.ReadModel,
                    new InsertOneOptions { BypassDocumentValidation = true },
                    cancellationToken);
            }
        }
    }
}
