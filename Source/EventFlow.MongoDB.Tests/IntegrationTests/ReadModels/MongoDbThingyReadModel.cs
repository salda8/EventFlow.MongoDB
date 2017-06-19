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

namespace EventFlow.MongoDB.Tests.IntegrationTests.ReadModels
{
    public class MongoDbThingyReadModel : IMongoDbReadModel,
        IAmReadModelFor<ThingyAggregate, ThingyId, ThingyDomainErrorAfterFirstEvent>,
        IAmReadModelFor<ThingyAggregate, ThingyId, ThingyPingEvent>
    {

        public bool DomainErrorAfterFirstReceived { get; set; }
        public int PingsReceived { get; set; }

        public long? Version { get; set; }

        public string _id { get; set; }

        public long? _version { get; set; }

        public void Apply(IReadModelContext context, IDomainEvent<ThingyAggregate, ThingyId, ThingyDomainErrorAfterFirstEvent> domainEvent)
        {
            _id = domainEvent.AggregateIdentity.Value;
            DomainErrorAfterFirstReceived = true;
        }

        public void Apply(IReadModelContext context, IDomainEvent<ThingyAggregate, ThingyId, ThingyPingEvent> domainEvent)
        {
            _id = domainEvent.AggregateIdentity.Value;
            PingsReceived++;
        }

        public Thingy ToThingy()
        {
            return new Thingy(
                ThingyId.With(_id),
                PingsReceived,
                DomainErrorAfterFirstReceived);
        }
    }
}
