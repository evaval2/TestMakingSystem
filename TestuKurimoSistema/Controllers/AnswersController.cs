using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
using Newtonsoft.Json;
using System.IO;
namespace TestuKurimoSistema.Controllers
{
    [Route("temos/{topicId:int}/testai/{testId:int}/klausimai/{questionId:int}/atsakymai")]
    public class AnswersController : Controller
    {
        private Answer _answer;
        private Dictionary<string, Dictionary<string, string>> parameters;
        private Error _error;
        public AnswersController()
        {
            _answer = new Answer();
        }
        private IActionResult checkParams(out string role)
        {
            role = "";
            string exception;
            _error = new Error(Response);

            if (!Request.IsHttps)
                return Json(_error.WriteError(500, "not_secure", "", "Use https instead of http."));
            var jsonSerializer = new JsonSerializer();
            if (Request != null)
            {
                using (var streamReader = new StreamReader(Request.Body))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    parameters = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonTextReader);
                }
            }
            if (parameters == null)
                parameters = new Dictionary<string, Dictionary<string, string>>();         

            if (!parameters.ContainsKey("Authorization"))
                return Json(_error.WriteError(400, "invalid_request", "", "Request does not contain object \"Authorization\"."));
            if (!parameters["Authorization"].ContainsKey("access_token"))
                return Json(_error.WriteError(400, "invalid_request", "", "Object \"Authorization\" does not contain item \"access_token\"."));
            if (!parameters["Authorization"].ContainsKey("token_type"))
                return Json(_error.WriteError(400, "invalid_request", "", "Object \"Authorization\" does not contain item \"token_type\"."));
            role = TokenValidator.Validate(parameters["Authorization"]["access_token"], out exception);
            if (role == null)
            {
                return Json(_error.WriteError(401, "unauthorized_client", "", "Invalid access token."));
            }
            return null;
        }
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(int questionId)
        {
            var list = await _answer.Select(questionId);
            return Ok(list);
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int questionId, int id)
        {
            var answer = await _answer.Select(questionId, id);
            if (answer != null)
                return Ok(answer);
            return NotFound();
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post(int questionId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId, int questionId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var value = construct();
                if (value == null)
                    return BadRequest();
                var answer = await _answer.Insert(questionId, value);
                return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/"
                                        + questionId + "/atsakymai/" + answer.Id, answer);
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpPatch]
        public async Task<IActionResult> Patch(int questionId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int questionId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int questionId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int questionId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var value = construct();
                value.Id = id;
                if (value == null)
                    return BadRequest();
                var answer = await _answer.Update(questionId, id, value);
                if (answer != null)
                {
                    return Ok(answer);
                }
                return NotFound();
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int questionId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var response = await _answer.Remove(questionId, id);
                if (response)
                {
                    return NoContent();
                }
                return NotFound();
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        private Answer construct()
        {
            if (!parameters.ContainsKey("Answer"))
                return null;
            if (!parameters["Answer"].ContainsKey("AnswerText") || !parameters["Answer"].ContainsKey("QuestionType") || !parameters["Answer"].ContainsKey("IsCorrect"))
            {
                return null;
            }
            var answer = new Answer(0, parameters["Answer"]["AnswerText"], 0, false);
            int type;
            if (int.TryParse(parameters["Answer"]["QuestionType"], out type))
                answer.QuestionType = type;
            bool isCorrect;
            if (bool.TryParse(parameters["Answer"]["IsCorrect"], out isCorrect))
                answer.IsCorrect = isCorrect;
            return answer;
        }
    }
}
