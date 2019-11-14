using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TestuKurimoSistema.DB.Entities
{
    public class Test
    {
        private int id { get; set; }
        private string name { get; set; }
        private float average { get; set; }
        private int time { get; set; }
        public Test() {}
        public Test(int i, string n, float a, int t)
        {
            this.id = i;
            this.name = n;
            this.average = a;
            this.time = t;
        }
        private Test readRow(Row row)
        {
            if (row != null)
            {
                return new Test(row.GetValue<int>("id"), 
                                row.GetValue<string>("name"), 
                                row.GetValue<float>("average"), 
                                row.GetValue<int>("time"));
            }
            return null;
        }
        public int Id {
            get { return id; }
            set { this.id = value;  } }
        public string TestName {
            get { return name; }
            set { this.name = value; } }
        public float AverageGrade {
            get { return average; }
            set { this.average = value; } }
        public int TestTime {
            get { return time; }
            set { this.time = value; } }
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
        public async Task<List<Test>> Select(int topic_id)
        {
            var tests = new List<Test>();
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT id, name, average, time " +
                                                "FROM test " +
                                                "WHERE topic_id = ?", topic_id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                tests = response.Select((r) => readRow(r)).ToList();
            }
            return tests;
        }
        public async Task<Test> Select(int topic_id, int id)
        {
            var test = new Test();            
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT id, name, average, time " +
                                                "FROM test " +
                                                "WHERE topic_id = ? AND id = ?", topic_id, id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                test = readRow(response.FirstOrDefault());
            }
            return test;
        }
        public async Task<Test> Insert(int topic_id, Test testB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT count FROM count " +
                                                "WHERE id = ?", "test");
                var response = await session.ExecuteAsync(query);
                var id = response.FirstOrDefault().GetValue<int>("count");                
                query = new SimpleStatement("INSERT INTO test (id, name, average, topic_id, time) " +
                                             "VALUES (" + id + ",'" + testB.TestName + "'," + testB.AverageGrade + "," +
                                                      topic_id + "," + testB.TestTime + ")");
                response = await session.ExecuteAsync(query);
                testB.Id = id;
                id++;
                query = new SimpleStatement("UPDATE count  SET count = " + id + " WHERE id = ?", "test");
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return testB;
        }
        public async Task<Test> Update(int topic_id, int id, Test testB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM test " +
                                                "WHERE topic_id = ? AND id = ?", topic_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("UPDATE test  " +
                                            "SET name = '" + testB.TestName + "', average = " + testB.AverageGrade + ", " +
                                            "time = " + testB.TestTime + " WHERE topic_id = ? AND id = ?", topic_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                testB.Id = id;
            }
            return testB;
        }
        public async Task<bool> Remove(int topic_id, int id)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM test " +
                                                "WHERE topic_id = ? AND id = ?", topic_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return false;
                }
                query = new SimpleStatement("DELETE FROM test " +
                                            "WHERE topic_id = ? AND id = ?", topic_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return true;
        }
    }
}
