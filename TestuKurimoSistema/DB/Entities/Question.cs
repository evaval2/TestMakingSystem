using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
namespace TestuKurimoSistema.DB.Entities
{
    public class Question
    {
        private int id { get; set; }
        private string question { get; set; }
        private int type { get; set; }
        public Question() { }
        public Question(int i, string q, int t)
        {
            this.id = i;
            this.question = q;
            this.type = t;
        }
        private Question readRow(Row row)
        {
            if (row != null)
            {
                return new Question(row.GetValue<int>("id"), 
                                    row.GetValue<string>("question"), 
                                    row.GetValue<int>("type"));
            }
            return null;
        }
        public int Id {
            get { return id; }
            set { this.id = value; } }
        public string QuestionText {
            get { return question; }
            set { this.question = value; } }
        public int QuestionType {
            get { return type; }
            set { this.type = value; } }
        public async Task<List<Question>> Select(int test_id)
        {
            var questions = new List<Question>();
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT id, question, type " +
                                                "FROM question WHERE test_id = ?", test_id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                questions = response.Select((r) => readRow(r)).ToList();
            }
            return questions;
        }
        public async Task<Question> Select(int test_id, int id)
        {
            var question = new Question();
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT id, question, type " +
                                                "FROM question " +
                                                "WHERE test_id = ? and id = ?", test_id, id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                question = readRow(response.FirstOrDefault());
            }
            return question;
        }
        public async Task<Question> Insert(int test_id, Question questionB)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT count FROM count where id = ?", "question");
                var response = await session.ExecuteAsync(query);
                var id = response.FirstOrDefault().GetValue<int>("count");                
                query = new SimpleStatement("INSERT INTO question (id, question, type, test_id) " +
                                            "VALUES (" + id + ",'" + questionB.QuestionText + "'," + 
                                                     questionB.QuestionType + ","+ test_id +")");
                response = await session.ExecuteAsync(query);
                questionB.Id = id;
                id++;
                query = new SimpleStatement("UPDATE count  SET count = " + id + 
                                           " WHERE id = ?", "question");
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return questionB;
        }
        public async Task<Question> Update(int test_id, int id, Question questionB)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM question " +
                                                "WHERE test_id = ? and id = ?", test_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("UPDATE question " +
                                            "SET question = '" + questionB.QuestionText + "', " +
                                            "type = " + questionB.QuestionType + " " +
                                            "WHERE test_id = ? and id = ?", test_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                questionB.Id = id;
            }
            return questionB;
        }
        public async Task<bool> Remove(int test_id, int id)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM question " +
                                                "WHERE test_id = ? and id = ?", test_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return false;
                }
                query = new SimpleStatement("DELETE FROM question " +
                                            "WHERE test_id = ? and id = ?", test_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return true;
        }
    }
}
