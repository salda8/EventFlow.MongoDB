using EventFlow.MongoDB.ReadStores;
using EventFlow.ReadStores;
using EventFlow.TestHelpers.Aggregates;
using EventFlow.TestHelpers.Aggregates.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventFlow.Aggregates;
using EventFlow.TestHelpers.Aggregates.Entities;

namespace EventFlow.MongoDB.Tests.IntegrationTests.ReadModels
{
    public class MongoDbThingyMessageReadmodel : IMongoDbReadModel,
        IAmReadModelFor<ThingyAggregate, ThingyId, ThingyMessageAddedEvent>

    {

        public string _id { get; set; }
        public long? _version { get; set; }
        public string ThingyId { get; set; }
        public string Message { get; set; }
        public void Apply(IReadModelContext context, IDomainEvent<ThingyAggregate, ThingyId, ThingyMessageAddedEvent> domainEvent)
        {
            ThingyId = domainEvent.AggregateIdentity.Value;
            var thingyMessage = domainEvent.AggregateEvent.ThingyMessage;
            _id = thingyMessage.Id.Value;
            Message = thingyMessage.Message;
        }

        public ThingyMessage ToThingyMessage()
        {
            return new ThingyMessage(
                ThingyMessageId.With(_id),
                Message);
        }
    }
}
