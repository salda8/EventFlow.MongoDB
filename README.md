# EventFlow.MongoDB

EventFlow.MongoDB offers MongoDB functionality to the [EventFlow](https://github.com/eventflow/EventFlow) package.

### Features
* ReadStores - create readmodels which are persisted in MongoDB
* SnapshotStore - create snapshots which are persisted in MongoDB
* EventStore - events are persited in MongoDB

### Installation
run the following command in the [Package Manager Console](https://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```Install-Package EventFlow.MongoDB```

### Usage
#### Configure MongoDB
Use the ConfigureMongoDb(mongoUrl, mongodb) extension method on the IEventFlowOptions.
```c#
var resolver = EventFlowOptions.New
    ...
    .ConfigureMongoDb(mongoUrl, mongodb)
    ...
    .CreateResolver(true);
```

Where mongoUrl is valid MongoDB [connection string](https://docs.mongodb.com/manual/reference/connection-string/) and mongodb the name of the database.

#### Create a read model
```c#
[MongoDbCollectionName("users")]
public class UserReadModel : IMongoDbReadModel,
  IAmReadModelFor<UserAggregate, UserId, UserCreated>
{   
  public string _id { get; set; }
  public long? _version { get; set; }
  public string Username { get; set; }

  public void Apply(
    IReadModelContext context,
    IDomainEvent<UserAggregate, UserId, UserCreated> domainEvent)
  {
    _id = domainEvent.AggregateIdentity.Value;
    Username = domainEvent.AggregateEvent.Username.Value;
  }
}
```
#### Create a ReadModelLocator if needed
```c#
public interface IUserReadModelLocator : IReadModelLocator { }

public class UserReadModelLocator : IUserReadModelLocator
{
    public IEnumerable<string> GetReadModelIds(IDomainEvent domainEvent)
    {
        yield return "some id based on some event";
    }
}
```
#### Register the readmodel
Without ReadModelLocator:
```c#
var resolver = EventFlowOptions.New
    ...
    .UseMongoDbReadModel<UserReadModel>()
    ...
    .CreateResolver(true);
```

With ReadModelLocator:
```c#
var resolver = EventFlowOptions.New
    ...
    .RegisterServices(sr =>
    {
        sr.Register<IUserReadModelLocator, UserReadModelLocator>();
    })

    .UseMongoDbReadModel<UserReadModel, IUserReadModelLocator>()
    ...
    .CreateResolver(true);
```
#### Configure EventFlow to use MongoDB for Snapshots
```c#
var resolver = EventFlowOptions.New
    ...
    .UseMongoDbSnapshotStore()
    ...
    .CreateResolver(true);
```
### 2 Types of ReadModels
There's currently 2 types of read models:
* IMongoDbReadModel
* IMongoDbInsertOnlyReadModel

Configuring EventFlow is identical for both, you only have to implement the proper interface to get the required functionality.

#### IMongoDbReadModel
Works as a normal IReadModel and it will update the document corresponding with the _id property.
#### IMongoDbInsertOnlyReadModel
Every event that is applied will result in a Mongo document being inserted in the collection. A use case for this would be an account ledger where you want to store every single transaction that happens.

#### Configure EventFlow to use MongoDB for Events
```c#
var resolver = EventFlowOptions.New
    ...
    .UseMongoDbEventStore()
    ...
    .CreateResolver(true);
```
