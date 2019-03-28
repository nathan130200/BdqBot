using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text;

namespace Bdq.Entities.QA
{
    [BsonCollection("bdq_questoes")]
    public class Questao
    {

        [BsonId]
        protected ObjectId _id;

        [BsonElement("question_id"), BsonRequired]
        public string Id { get; set; }

        [BsonElement("category"), BsonRequired]
        public string Categoria { get; set; }

        [BsonElement("sub_category"), BsonRequired]
        public string SubCategoria { get; set; }

        [BsonElement("text")]
        public string Texto { get; set; }

        [BsonElement("answers")]
        public List<QuestaoAlternativa> Alternativas { get; set; }

        [BsonElement("points"), BsonDefaultValue(1)]
        public int Pontos { get; set; }
    }
}
