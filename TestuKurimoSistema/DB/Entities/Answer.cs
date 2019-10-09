using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
namespace TestuKurimoSistema.DB.Entities
{
    public class Answer
    {
        private int id { get; set; }
        private string answer { get; set; }
        private int type { get; set; }
        private bool correct { get; set; }
        public Answer() { }
        public Answer(int i, string a, int t, bool c)
        {
            this.id = i;
            this.answer = a;
            this.type = t;
            this.correct = c;
        }
        private Answer readRow(Row row)
        {
            if (row != null)
            {
                return new Answer(row.GetValue<int>("id"), 
                                  row.GetValue<string>("answer"), 
                                  row.GetValue<int>("type"), 
                                  row.GetValue<bool>("correct"));
            }
            return null;
        }
        public int Id {
            get { return id; }
            set { this.id = value; } }
        public string AnswerText {
            get { return answer; }
            set { this.answer = value; } }
        public int QuestionType {
            get { return type; }
            set { this.type = value; } }
        public bool IsCorrect {
            get { return correct; }
            set { this.correct = value; } }
        public async Task<List<Answer>> Select(int question_id)
        {
            var answers = new List<Answer>();
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT id, answer, type, correct " +
                                                "FROM answer WHERE question_id = ?", question_id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                answers = response.Select((r) => readRow(r)).ToList();
            }
            return answers;
        }
        public async Task<Answer> Select(int question_id, int id)
        {
            var answer = new Answer();
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT id, answer, type, correct " +
                                                "FROM answer " +
                                                "WHERE question_id = ? and id = ?", question_id, id);
                var response = await session.ExecuteAsync(query);
                session.Dispose();
                answer = readRow(response.FirstOrDefault());
            }
            return answer;
        }
        public async Task<Answer> Insert(int question_id, Answer answerB)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT count FROM count WHERE id = ?", "answer");
                var response = await session.ExecuteAsync(query);
                var id = response.FirstOrDefault().GetValue<int>("count");                
                query = new SimpleStatement("INSERT INTO answer (id, answer, type, correct, question_id) " +
                                            "VALUES (" + id + ",'" + answerB.AnswerText + "'," + 
                                                     answerB.QuestionType + "," + answerB.IsCorrect+ "," + 
                                                     question_id + ")");
                response = await session.ExecuteAsync(query);
                answerB.Id = id;
                id++;
                query = new SimpleStatement("UPDATE count  SET count = " + id + " WHERE id = ?", "answer");
                response = await session.ExecuteAsync(query);
                session.Dispose();                
            }
            return answerB;
        }
        public async Task<Answer> Update(int question_id, int id, Answer answerB)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c FROM answer " +
                                                "WHERE question_id = ? and id = ?", question_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return null;
                }
                query = new SimpleStatement("UPDATE answer  " +
                                            "SET answer = '" + answerB.AnswerText + "', " +
                                                "type = " + answerB.QuestionType + ", " +
                                                "correct = " + answerB.IsCorrect + " " +
                                            "WHERE question_id = ? and id = ?", question_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
                answerB.Id = id;
            }
            return answerB;
        }
        public async Task<bool> Remove(int question_id, int id)
        {
            var _cluster = Cluster.Builder().AddContactPoint("127.0.0.1")
                                            .WithPort(9042)
                                            .WithCredentials("cassandra", "casandra")
                                            .Build();
            using (var session = _cluster.Connect("testmakingsystem"))
            {
                var query = new SimpleStatement("SELECT COUNT(*) AS c " +
                                                "FROM answer " +
                                                "WHERE question_id = ? and id = ?", question_id, id);
                var response = await session.ExecuteAsync(query);
                var count = response.FirstOrDefault().GetValue<Int64>("c");
                if (count == 0)
                {
                    session.Dispose();
                    return false;
                }
                query = new SimpleStatement("DELETE FROM answer " +
                                            "WHERE question_id = ? and id = ?", question_id, id);
                response = await session.ExecuteAsync(query);
                session.Dispose();
            }
            return true;
        }
    }
}

