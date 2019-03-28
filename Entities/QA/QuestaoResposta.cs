using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bdq.Entities.QA
{
    public class QuestaoAlternativa
    {
        [BsonElement("is_valid"), BsonDefaultValue(false), BsonIgnoreIfDefault]
        public bool Valida { get; set; } = false;

        [BsonElement("content"), BsonRequired]
        public string Texto { get; set; }
    }
}
