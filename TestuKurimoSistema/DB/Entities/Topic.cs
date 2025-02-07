﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TestuKurimoSistema.DB.Entities
{
    public class Topic
    {
        private int id { get; set; }
        private string name { get; set; }
        public Topic() {}
        public Topic(int i, string n)
        {
            this.id = i;
            this.name = n;
        }
        private Topic readRow(Row row)
        {
            if (row != null)
            {
                return new Topic(row.GetValue<int>("id"), 
                                 row.GetValue<string>("name"));
            }
            return null;
        }
        public string TopicName {
            get { return name; }
            set { this.name = value; } }
        public int Id {
            get { return id; }
            set { this.id = value; } }
        private static bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
        private static ISession getSession()
        {            
            Cluster _cluster = Cluster.Builder()
                .WithCredentials("cassandra", "cassandra")
                .WithPort(9042)
                .AddContactPoint("localhost")
                .Build();
            ISession session = _cluster.Connect("testmakingsystem");
            return session;
        }
        public async Task<List<Topic>> Select()
        {
            var topics = new List<Topic>();
            using (var session = getSession())
            {            
                var query = new SimpleStatement("SELECT id, name FROM topic");
                var response = await session.ExecuteAsync(query);
                topics = response.Select((r) => readRow(r)).ToList();
                session.Dispose();                
            }
            return topics;
        }
        
        public async Task<Topic> Select(int id)
        {
            var topic = new Topic();
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT id, name " +
                                                "FROM topic " +
                                                "WHERE id = ?", id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                topic = readRow(response.FirstOrDefault());
            }
            return topic;
        }
        public async Task<Topic> Insert(Topic topicB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c " +
                                                "FROM topic " +
                                                "WHERE name = ?", topicB.TopicName);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count != 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("SELECT count FROM count " +
                                            "WHERE id = ?", "topic");
                response = await session.ExecuteAsync(query);
                var id = response.FirstOrDefault().GetValue<int>("count");
                query = new SimpleStatement("INSERT INTO topic (id, name) " +
                                            "VALUES (" + id + ",'" + topicB.TopicName + "')");
                topicB.Id = id;
                id++;
                response = await session.ExecuteAsync(query);
                query = new SimpleStatement("UPDATE count  SET count = " + id + " " +
                                            "WHERE id = ?","topic");
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return topicB;
        }
        public async Task<Topic> Update(int id, Topic topicB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM topic " +
                                                "WHERE id = ?", id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");                
                if (count == 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("SELECT COUNT(*) AS c FROM topic " +
                                            "WHERE name = ?", topicB.TopicName);
                response = await session.ExecuteAsync(query);
                count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count != 0)
                {
                    return new Topic(-1, "");
                }
                query = new SimpleStatement("UPDATE topic  " +
                                            "SET name = '" + topicB.TopicName + "' " +
                                            "WHERE id = ?", id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                topicB.Id = id;
            }
            return topicB;
        }
        public async Task<bool> Remove(int id)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM topic WHERE id = ?", id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return false;
                }
                query = new SimpleStatement("DELETE FROM topic WHERE id = ?", id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return true;
        }
    }
}
