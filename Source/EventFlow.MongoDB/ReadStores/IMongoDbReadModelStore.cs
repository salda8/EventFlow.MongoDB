using EventFlow.ReadStores;

namespace EventFlow.MongoDB.ReadStores
{
    public interface IMongoDbReadModelStore<TReadModel> : IReadModelStore<TReadModel>
        where TReadModel : class, IReadModel, new()
    {
    }
}
