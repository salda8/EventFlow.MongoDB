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
        private readonly IReadModelFactory<TReadModel> readModelFactory;

        public MongoDbInsertOnlyReadModelStore(
            ILog log,
            IMongoDatabase mongoDatabase,
            IInsertOnlyReadModelDescriptionProvider readModelDescriptionProvider, IReadModelFactory<TReadModel> readModelFactory)
        {
            _log = log;
            _mongoDatabase = mongoDatabase;
            _readModelDescriptionProvider = readModelDescriptionProvider;
            this.readModelFactory = readModelFactory;
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Information($"Deleting '{typeof(TReadModel).PrettyPrint()}' with id '{id}', from '{readModelDescription.RootCollectionName}'!");

            var collection = _mongoDatabase.GetCollection<TReadModel>(readModelDescription.RootCollectionName.Value);
            await collection.DeleteOneAsync(x => x._id.ToString() == id, cancellationToken);
        }

        public async Task DeleteAllAsync(CancellationToken cancellationToken)
        {
            var readModelDescription = _readModelDescriptionProvider.GetReadModelDescription<TReadModel>();

            _log.Information($"Deleting ALL '{typeof(TReadModel).PrettyPrint()}' by DROPPING COLLECTION '{readModelDescription.RootCollectionName}'!");

            await _mongoDatabase.DropCollectionAsync(readModelDescription.RootCollectionName.Value, cancellationToken);
        }

        public Task<ReadModelEnvelope<TReadModel>> GetAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(ReadModelEnvelope<TReadModel>.Empty(id));
        }

        public async Task UpdateAsync(IReadOnlyCollection<ReadModelUpdate> readModelUpdates, IReadModelContextFactory readModelContextFactory, Func<IReadModelContext, IReadOnlyCollection<IDomainEvent>, ReadModelEnvelope<TReadModel>, CancellationToken, Task<ReadModelUpdateResult<TReadModel>>> updateReadModel, CancellationToken cancellationToken)
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
                var readModel = await readModelFactory.CreateAsync(readModelUpdate.ReadModelId, cancellationToken).ConfigureAwait(false);

                var readModelEnvelope = ReadModelEnvelope<TReadModel>.With(readModelUpdate.ReadModelId, readModel);

                ReadModelUpdateResult<TReadModel> readModelUpdateResult = await updateReadModel?.Invoke(readModelContextFactory.Create(readModelUpdate.ReadModelId, false), readModelUpdate.DomainEvents, readModelEnvelope, cancellationToken);
                readModelEnvelope.ReadModel._id = ObjectIdGenerator.Instance.GenerateId(collection, readModelEnvelope.ReadModel);
                await collection.InsertOneAsync(
                    readModelEnvelope.ReadModel,
                    new InsertOneOptions { BypassDocumentValidation = true },
                    cancellationToken);
            }
        }
    }
}