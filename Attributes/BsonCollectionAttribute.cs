using System;

namespace MongoDB.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class BsonCollectionAttribute : Attribute
    {
        public string Name { get; }

        public BsonCollectionAttribute(string collection)
        {
            this.Name = collection;
        }
    }
}
