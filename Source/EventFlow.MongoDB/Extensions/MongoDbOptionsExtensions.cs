﻿using EventFlow.Configuration;
using EventFlow.Extensions;
using EventFlow.MongoDB.ReadStores;
using EventFlow.ReadStores;
using EventFlow.MongoDB.EventStore;
using MongoDB.Driver;
using System;
using EventFlow.Aggregates;
using EventFlow.Core;

namespace EventFlow.MongoDB.Extensions
{
    public static class MongoDbOptionsExtensions
    {
        public static IEventFlowOptions ConfigureMongoDb(
            this IEventFlowOptions eventFlowOptions,
            string url,
            string database)
        {
            var mongoUrl = new MongoUrl(url);
            var mongoClient = new MongoClient(mongoUrl);
            return eventFlowOptions
                .ConfigureMongoDb(mongoClient, database);
        }

        public static IEventFlowOptions ConfigureMongoDb(
            this IEventFlowOptions eventFlowOptions,
            string database)
        {
            var mongoClient = new MongoClient();
            return eventFlowOptions
                .ConfigureMongoDb(mongoClient, database);
        }

        public static IEventFlowOptions ConfigureMongoDb(
            this IEventFlowOptions eventFlowOptions,
            IMongoClient mongoClient,
            string database)
        {
            IMongoDatabase mongoDatabase = mongoClient.GetDatabase(database);
            return eventFlowOptions.ConfigureMongoDb(() => mongoDatabase);
        }

        public static IEventFlowOptions ConfigureMongoDb(
            this IEventFlowOptions eventFlowOptions,
            Func<IMongoDatabase> mongoDatabaseFactory)
        {
            return eventFlowOptions.RegisterServices(sr =>
            {
                sr.Register(f => mongoDatabaseFactory?.Invoke(), Lifetime.Singleton);
                sr.Register<IReadModelDescriptionProvider, ReadModelDescriptionProvider>(Lifetime.Singleton, true);
                sr.Register<IInsertOnlyReadModelDescriptionProvider, InsertOnlyReadModelDescriptionProvider>(Lifetime.Singleton, true);
                sr.Register<IMongoDbEventSequenceStore, MongoDbEventSequenceStore>(Lifetime.Singleton);
            });
        }

        public static IEventFlowOptions UseMongoDbReadModel<TReadModel>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IMongoDbReadModel, new()
        {
            return eventFlowOptions
                .RegisterServices(f =>
                {
                    f.Register<IMongoDbReadModelStore<TReadModel>, MongoDbReadModelStore<TReadModel>>();
                    f.Register<IReadModelStore<TReadModel>>(r => r.Resolver.Resolve<IMongoDbReadModelStore<TReadModel>>());
                })
                .UseReadStoreFor<IMongoDbReadModelStore<TReadModel>, TReadModel>();
        }

        public static IEventFlowOptions UseMongoDbReadModel<TReadModel, TReadModelLocator>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IMongoDbReadModel, new()
            where TReadModelLocator : IReadModelLocator
        {
            return eventFlowOptions
                .RegisterServices(f =>
                {
                    f.Register<IMongoDbReadModelStore<TReadModel>, MongoDbReadModelStore<TReadModel>>();
                    f.Register<IReadModelStore<TReadModel>>(r => r.Resolver.Resolve<IMongoDbReadModelStore<TReadModel>>());
                })
                .UseReadStoreFor<IMongoDbReadModelStore<TReadModel>, TReadModel, TReadModelLocator>();
        }

        public static IEventFlowOptions UseMongoDbInsertOnlyReadModel<TReadModel>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IMongoDbInsertOnlyReadModel, new()
        {
            return eventFlowOptions
                .RegisterServices(f =>
                {
                    f.Register<IMongoDbInsertOnlyReadModelStore<TReadModel>, MongoDbInsertOnlyReadModelStore<TReadModel>>();
                    f.Register<IReadModelStore<TReadModel>>(r => r.Resolver.Resolve<IMongoDbInsertOnlyReadModelStore<TReadModel>>());
                })
                .UseReadStoreFor<IMongoDbInsertOnlyReadModelStore<TReadModel>, TReadModel>();
        }

        public static IEventFlowOptions UseMongoDbInsertOnlyReadModel<TReadModel, TReadModelLocator>(
            this IEventFlowOptions eventFlowOptions)
            where TReadModel : class, IMongoDbInsertOnlyReadModel, new()
            where TReadModelLocator : IReadModelLocator
        {
            return eventFlowOptions
                .RegisterServices(f =>
                {
                    f.Register<IMongoDbInsertOnlyReadModelStore<TReadModel>, MongoDbInsertOnlyReadModelStore<TReadModel>>();
                    f.Register<IReadModelStore<TReadModel>>(r => r.Resolver.Resolve<IMongoDbInsertOnlyReadModelStore<TReadModel>>());
                })
                .UseReadStoreFor<IMongoDbInsertOnlyReadModelStore<TReadModel>, TReadModel, TReadModelLocator>();
        }
        public static IEventFlowOptions UseMongoReadStoreFor<TAggregate, TIdentity, TReadStore, TReadModel>(this IEventFlowOptions eventFlowOptions)
                where TAggregate : IAggregateRoot<TIdentity>
                where TIdentity : IIdentity
                where TReadStore : class, IReadModelStore<TReadModel>
                where TReadModel : class, IMongoDbReadModel, new()

        {
            return eventFlowOptions
                    .RegisterServices(f =>
                    {
                        f.Register<IMongoDbReadModelStore<TReadModel>, MongoDbReadModelStore<TReadModel>>();
                        f.Register<IReadModelStore<TReadModel>>(r => r.Resolver.Resolve<IMongoDbReadModelStore<TReadModel>>());
                    })
                    .UseReadStoreFor<TAggregate, TIdentity, TReadStore, TReadModel>();

        }

    }
}
