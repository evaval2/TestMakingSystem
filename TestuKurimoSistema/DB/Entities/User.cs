using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
namespace TestuKurimoSistema.DB.Entities
{
    public class User
    {
        private string username { get; set; }
        private string password { get; set; }
        private string role { get; set; }
        private float average { get; set; }
        public User() { }
        public User(string u, string p, string r, float a)
        {
            this.username = u;
            this.password = p;
            this.role = r;
            this.average = a;
        }
        private User readRow(Row row)
        {
            if (row != null)
            {
                return new User(row.GetValue<string>("username"), 
                                row.GetValue<string>("password"), 
                                row.GetValue<string>("role"), 
                                row.GetValue<float>("average"));
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
        public async Task<List<User>> Select()
        {
            var users = new List<User>();
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT username, password, role, average " +
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
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT username, password, role, average " +
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
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
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
                query = new SimpleStatement("INSERT INTO user (username, password, role, average) " +
                                            "VALUES ('" + userB.Username + "','" + userB.Password + "','" + 
                                                          userB.UserRole + "'," + userB.AverageGrade + ")");
                response = await session.ExecuteAsync(query);
                query = new SimpleStatement("UPDATE count  SET count = " + count + " WHERE id = ?", "user");
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return userB;
        }
        public async Task<User> Update(string id, User userB)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
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
                                                          "', average = " + userB.AverageGrade + " WHERE username = ?", id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                userB.Username = id;
            }
            return userB;
        }
        public async Task<bool> Remove(string id)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
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
