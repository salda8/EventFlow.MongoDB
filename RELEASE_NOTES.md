### New in 0.1 (not released yet)

* New: Added event storage in MongoDB by implamenting `IEventPersistence`
* New: Provided `DeleteAsync` in the `IMongoDbReadModelStore<TReadModel>`.
* New: Provided `FindAsync` in the `IMongoDbReadModelStore<TReadModel>`.
* Fixed: Null reference in counter when saving an event in a new database.