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
    public class User
    {
        private string username { get; set; }
        private string password { get; set; }
        private string role { get; set; }
        private float average { get; set; }
        public string Token { get; set; }
        public User() { }
        public User(string u, string p, string r, float a, string t)
        {
            this.username = u;
            this.password = p;
            this.role = r;
            this.average = a;
            this.Token = t;
        }
        private User readRow(Row row)
        {
            if (row != null)
            {
                return new User(row.GetValue<string>("username"), 
                                row.GetValue<string>("password"), 
                                row.GetValue<string>("role"), 
                                row.GetValue<float>("average"),
                                row.GetValue<string>("refresh_token"));
            }
            return null;
        }
        public string Username {
            get { return username; }
            set { this.username = value; } }
        public string Password {
            get { return password; }
            set { this.password = value; } }
        public string UserRole {
            get { return role; }
            set { this.role = value; } }
        public float AverageGrade {
            get { return average; }
            set { this.average = value; } }
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
        public async Task<List<User>> Select()
        {
            var users = new List<User>();
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT username, password, role, average, refresh_token " +
                                                "FROM user");
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                users = response.Select((r) => readRow(r)).ToList();                
            }
            return users;
        }
        public async Task<User> Select(string id)
        {
            var user = new User();
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT username, password, role, average, refresh_token " +
                                                "FROM user " +
                                                "WHERE username = ?", id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                user = readRow(response.FirstOrDefault());
            }
            return user;
        }
        public async Task<User> Insert(User userB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c " +
                                                "FROM user " +
                                                "WHERE username = ?", userB.Username);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count != 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("SELECT count FROM count WHERE id = ?", "user");
                response = await session.ExecuteAsync(query);
                count = response.FirstOrDefault().GetValue<int>("count");
                count++;
                query = new SimpleStatement("INSERT INTO user (username, password, role, average, refresh_token) " +
                                            "VALUES ('" + userB.Username + "','" + userB.Password + "','" + 
                                                          userB.UserRole + "'," + userB.AverageGrade + ","+ userB.Token + ")");
                response = await session.ExecuteAsync(query);
                query = new SimpleStatement("UPDATE count  SET count = " + count + " WHERE id = ?", "user");
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return userB;
        }
        public async Task<User> Update(string id, User userB)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM user WHERE username = ?", id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("UPDATE user  SET password = '" + userB.Password + "', role = '" + userB.UserRole + 
                                                          "', average = " + userB.AverageGrade + ", , refresh_token = "+ userB.Token + " WHERE username = ?", id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                userB.Username = id;
            }
            return userB;
        }
        public async Task<bool> Remove(string id)
        {
            using (var session = getSession())
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM user WHERE username = ?", id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return false;
                }
                query = new SimpleStatement("DELETE FROM user WHERE username = ?", id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return true;
        }
    }
}
