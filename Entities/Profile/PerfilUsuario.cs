using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bdq.Entities.Profile
{
    [BsonCollection("bdq_profiles")]
    public class PerfilUsuario
    {
        [BsonId]
        protected ObjectId _id;

        [BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public ulong Id { get; set; }

        [BsonElement("experience"), BsonRepresentation(BsonType.String), BsonDefaultValue(0)]
        public ulong Experience { get; set; }

        [BsonElement("points"), BsonRepresentation(BsonType.String), BsonDefaultValue(0)]
        public ulong Pontos { get; set; }
    }
}
