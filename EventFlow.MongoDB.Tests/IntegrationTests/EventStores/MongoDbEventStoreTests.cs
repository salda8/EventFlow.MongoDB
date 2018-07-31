using EventFlow.Configuration;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlow.Extensions;
using EventFlow.MongoDB.EventStore;
using EventFlow.MongoDB.Extensions;
using NUnit.Framework;
using Mongo2Go;

namespace EventFlow.MongoDB.Tests.IntegrationTests.EventStores
{
	[Category(Categories.Integration)]
	[TestFixture]
	public class MongoDbEventStoreTests : TestSuiteForEventStore
	{
		private MongoDbRunner _runner;
		
		protected override IRootResolver CreateRootResolver(IEventFlowOptions eventFlowOptions)
		{
			var resolver = eventFlowOptions
				.ConfigureMongoDb(_runner.ConnectionString, "eventflow")
				.UseEventStore<MongoDbEventPersistence>()
				.CreateResolver();
			
			return resolver;
		}

		[Test]
		public void Foo()
		{
			Assert.True(true);
		}

		[SetUp]
		public void SetUp()
		{
			_runner = MongoDbRunner.StartForDebugging();
		}

		[TearDown]
		public void TearDown()
		{
			_runner.Dispose();
		}
	}
}
