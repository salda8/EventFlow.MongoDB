using EventFlow.Configuration;
using EventFlow.TestHelpers;
using EventFlow.TestHelpers.Suites;
using EventFlow.Extensions;
using EventFlow.MongoDB.EventStore;
using EventFlow.MongoDB.Extensions;
using Xunit;
using Mongo2Go;
using System;

namespace EventFlow.MongoDB.Tests.IntegrationTests.EventStores
{
		
	public class MongoDbEventStoreTests : TestSuiteForEventStore, IDisposable
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

	   [Fact]
		public void Foo()
		{
			Assert.True(true);
		}

		public MongoDbEventStoreTests()
		{
			_runner = MongoDbRunner.Start();
		}
	
		public void Dispose(){
			_runner.Dispose();
		}
	}
}
