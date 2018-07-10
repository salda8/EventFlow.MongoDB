using EventFlow.ValueObjects;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventFlow.MongoDB
{
    internal class BsonClassMapping
    {

        private static void RegisterClassMaps()
        {
            BsonClassMap.RegisterClassMap<SingleValueObject<string>>(cm =>
            {
                cm.MapMember(x => x.Value);
                //cm.SetIsRootClass(true);
            });
            BsonClassMap.RegisterClassMap<SingleValueObject<DateTime>>(cm =>
            {
                cm.MapMember(x => x.Value);
                //cm.SetIsRootClass(true);
            });
            
        }
    }


}
