using EventFlow.MongoDB.ReadStores;
using EventFlow.TestHelpers;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventFlow.MongoDB.Tests.UnitTests
{
    [Category(Categories.Unit)]
    public class ReadModelDescriptionProviderTests : TestsFor<ReadModelDescriptionProvider>
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestReadModelA : IMongoDbReadModel
        {
            public string _id { get; set; }

            public long? _version { get; set; }
        }

        [MongoDbCollectionNameAttribute("SomeThingFancy")]
        private class TestReadModelB : IMongoDbReadModel
        {
            public string _id { get; set; }

            public long? _version { get; set; }
        }

        [Test]
        public void ReadModelCollectionIsCorrectWithoutAttribute()
        {
            // Act
            var readModelDescription = Sut.GetReadModelDescription<TestReadModelA>();

            // Assert
            readModelDescription.RootCollectionName.Value.Should().Be("eventflow-testreadmodela");
        }

        [Test]
        public void ReadModelIndexIsCorrectWithAttribute()
        {
            // Act
            var readModelDescription = Sut.GetReadModelDescription<TestReadModelB>();

            // Assert
            readModelDescription.RootCollectionName.Value.Should().Be("SomeThingFancy");
        }
    }
}
