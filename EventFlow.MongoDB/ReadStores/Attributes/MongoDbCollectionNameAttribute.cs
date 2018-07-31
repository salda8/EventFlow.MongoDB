using System;

namespace EventFlow.MongoDB.ReadStores.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MongoDbCollectionNameAttribute : Attribute
    {
        private string collectionName;
        public MongoDbCollectionNameAttribute(string collectionName)
        {
            this.collectionName = collectionName;
        }

        public string CollectionName
        {
            get { return collectionName; }
        }

    }
}
