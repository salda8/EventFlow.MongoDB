﻿using EventFlow.Core;
using EventFlow.Extensions;
using EventFlow.Logs;
using EventFlow.MongoDB.ValueObjects;
using EventFlow.Snapshots;
using EventFlow.Snapshots.Stores;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventFlow.MongoDB.SnapshotStores
{
    public class MongoDbSnapshotPersistence : ISnapshotPersistence
    {
        private static string SnapShotsCollectionName = "snapShots";
        private readonly ILog _log;
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbSnapshotPersistence(
            ILog log,
            IMongoDatabase mongoDatabase)
        {
            _log = log;
            _mongoDatabase = mongoDatabase;
        }

        public async Task<CommittedSnapshot> GetSnapshotAsync(
            Type aggregateType,
            IIdentity identity,
            CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<MongoDbSnapshotDataModel>(SnapShotsCollectionName);
            var filterBuilder = Builders<MongoDbSnapshotDataModel>.Filter;

            var filter = filterBuilder.Eq(model => model.AggregateName, aggregateType.GetAggregateName().Value) &
                         filterBuilder.Eq(model => model.AggregateId, identity.Value);

            var sort = Builders<MongoDbSnapshotDataModel>.Sort.Descending(model => model.AggregateSequenceNumber);
            var mongoDbSnapshotDataModel = await collection
                .Find(filter)
                .Sort(sort)
                .Limit(1)
                .FirstOrDefaultAsync();
            if (mongoDbSnapshotDataModel == null)
            {
                return null;
            }

            return new CommittedSnapshot(
                mongoDbSnapshotDataModel.Metadata,
                mongoDbSnapshotDataModel.Data);
        }

        public async Task SetSnapshotAsync(
            Type aggregateType,
            IIdentity identity,
            SerializedSnapshot serializedSnapshot,
            CancellationToken cancellationToken)
        {
            var mongoDbSnapshotDataModel = new MongoDbSnapshotDataModel
            {
                _id = ObjectId.GenerateNewId(DateTime.UtcNow),
                AggregateId = identity.Value,
                AggregateName = aggregateType.GetAggregateName().Value,
                AggregateSequenceNumber = serializedSnapshot.Metadata.AggregateSequenceNumber,
                Metadata = serializedSnapshot.SerializedMetadata,
                Data = serializedSnapshot.SerializedData,
            };

            var collection = _mongoDatabase.GetCollection<MongoDbSnapshotDataModel>(SnapShotsCollectionName);
            var filterBuilder = Builders<MongoDbSnapshotDataModel>.Filter;

            var filter = filterBuilder.Eq(model => model.AggregateName, aggregateType.GetAggregateName().Value) &
                         filterBuilder.Eq(model => model.AggregateId, identity.Value) &
                         filterBuilder.Eq(model => model.AggregateSequenceNumber, serializedSnapshot.Metadata.AggregateSequenceNumber);

            await collection.DeleteManyAsync(filter);
            await collection.InsertOneAsync(mongoDbSnapshotDataModel);
        }

        public Task DeleteSnapshotAsync(
            Type aggregateType,
            IIdentity identity,
            CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<MongoDbSnapshotDataModel>(SnapShotsCollectionName);
            var filterBuilder = Builders<MongoDbSnapshotDataModel>.Filter;

            var filter = filterBuilder.Eq(model => model.AggregateName, aggregateType.GetAggregateName().Value) &
                         filterBuilder.Eq(model => model.AggregateId, identity.Value);
            return collection.DeleteManyAsync(filter);
        }

        public Task PurgeSnapshotsAsync(CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<MongoDbSnapshotDataModel>(SnapShotsCollectionName);
            var filter = new BsonDocument();
            return collection.DeleteManyAsync(filter);
        }

        public Task PurgeSnapshotsAsync(
            Type aggregateType, 
            CancellationToken cancellationToken)
        {
            var collection = _mongoDatabase.GetCollection<MongoDbSnapshotDataModel>(SnapShotsCollectionName);
            var filter = Builders<MongoDbSnapshotDataModel>.Filter.Eq(model => model.AggregateName, aggregateType.GetAggregateName().Value);
            return collection.DeleteManyAsync(filter);
        }        
    }
}
