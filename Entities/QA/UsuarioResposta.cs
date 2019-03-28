using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bdq.Entities.QA
{
    [BsonCollection("bdq_respostas")]
    public class UsuarioResposta
    {
        [BsonId]
        protected ObjectId _id;

        [BsonElement("answer_id")]
        public string Id { get; set; }

        [BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public ulong UsuarioId { get; set; }

        [BsonElement("question_id")]
        public string QuestaoId { get; set; }

        [BsonElement("index")]
        public int Opcao { get; set; }

        [BsonElement("valid")]
        public bool Acertou { get; set; }
    }
}
