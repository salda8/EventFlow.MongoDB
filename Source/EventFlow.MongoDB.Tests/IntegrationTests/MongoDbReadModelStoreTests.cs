using EventFlow.TestHelpers.Suites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventFlow.Configuration;
using EventFlow.MongoDB.ReadStores;
using EventFlow.MongoDB.ValueObjects;
using EventFlow.TestHelpers.Aggregates.Entities;
using EventFlow.MongoDB.Extensions;
using EventFlow.MongoDB.Tests.IntegrationTests.ReadModels;
using EventFlow.Extensions;
using EventFlow.MongoDB.Tests.IntegrationTests.QueryHandlers;
using System.Threading;
using NUnit.Framework;
using EventFlow.TestHelpers;

namespace EventFlow.MongoDB.Tests.IntegrationTests
{
    [Category(Categories.Integration)]
    public class MongoDbReadModelStoreTests : TestSuiteForReadModelStore
    {
        protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
        {
            var testReadModelDescriptionProvider = new ReadModelDescriptionProvider();

            var resolver = eventFlowOptions
                .RegisterServices(sr =>
                {
                    sr.RegisterType(typeof(ThingyMessageLocator));
                    sr.Register<IReadModelDescriptionProvider>(c => testReadModelDescriptionProvider);
                })
                .ConfigureMongoDb("eventflow-test")
                .UseMongoDbReadModel<MongoDbThingyReadModel>()
                .UseMongoDbReadModel<MongoDbThingyMessageReadmodel, ThingyMessageLocator>()
                .AddQueryHandlers(
                    typeof(MongoDbThingyGetQueryHandler),
                    typeof(MongoDbThingyGetVersionQueryHandler),
                    typeof(MongoDbThingyGetMessagesQueryHandler))
                .CreateResolver();

            return resolver;
        }

        protected override Task PopulateTestAggregateReadModelAsync()
        {
            return ReadModelPopulator.PopulateAsync<MongoDbThingyReadModel>(CancellationToken.None);
        }

        protected override Task PurgeTestAggregateReadModelAsync()
        {
            return ReadModelPopulator.PurgeAsync<MongoDbThingyReadModel>(CancellationToken.None);
        }
    }
}
