using Bdq.Entities.Profile;
using Bdq.Entities.QA;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Bdq.Services
{
    public class DatabaseService
    {
        private MongoClient _client;

        public DatabaseService()
        {
            var settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress("127.0.0.1", 27017);

            settings.ClusterConfigurator = cluster =>
            { 
                var ts = new TraceSource("[MongoDB]",

#if DEBUG
                    SourceLevels.All
#else
                    SourceLevels.Warning
#endif
                    );
                ts.Listeners.Add(new TextWriterTraceListener(Console.Out));
                cluster.TraceWith(ts);
            };

            _client = new MongoClient(settings);
        }

        

        internal string GetCollectionName<T>()
        {
            var attr = typeof(T)
                .GetCustomAttribute<BsonCollectionAttribute>();

            if (attr == null)
                return $"bdq_{typeof(T).Name.ToLower()}";

            return attr.Name;
        }

        internal IMongoCollection<T> GetCollection<T>()
        {
            return this.Database.GetCollection<T>(GetCollectionName<T>());
        }

        public IMongoDatabase Database
        {
            get
            {
                return _client.GetDatabase("bdq");
            }
        }

        public IMongoCollection<Questao> Questions
        {
            get
            {
                return this.GetCollection<Questao>();
            }
        }

        public IMongoCollection<PerfilUsuario> Profiles
        {
            get
            {
                return this.GetCollection<PerfilUsuario>();
            }
        }

        public IMongoCollection<UsuarioResposta> Answers
        {
            get
            {
                return this.GetCollection<UsuarioResposta>();
            }
        }

        public IMongoCollection<UsuarioSimulado> Simulados
        {
            get
            {
                return this.GetCollection<UsuarioSimulado>();
            }
        }
    }
}