using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Bdq.Entities.QA
{
    [BsonCollection("bdq_simulados")]
    public class UsuarioSimulado
    {
        [BsonId]
        protected ObjectId _id;

        [BsonElement("simulado_id")]
        public string Id { get; set; }

        [BsonElement("user_id"), BsonRepresentation(BsonType.String)]
        public ulong UsuarioId { get; set; }

        [BsonElement("answers")]
        public List<UsuarioResposta> Respostas { get; set; }
    }
}
