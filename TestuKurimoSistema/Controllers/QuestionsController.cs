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
    [Route("temos/{topicId:int}/testai/{testId:int}/klausimai")]
    public class QuestionsController : Controller
    {
        private Question _question;
        private Dictionary<string, Dictionary<string, string>> parameters;
        private Error _error;
        public QuestionsController()
        {
            _question = new Question();
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
        public async Task<IActionResult> Get(int testId)
        {
            var list = await _question.Select(testId);
            return Ok(list);
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int testId, int id)
        {
            var question = await _question.Select(testId, id);
            if (question != null)
                return Ok(question);
            return NotFound();
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post(int topicId, int testId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId)
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
                var question = await _question.Insert(testId, value);
                return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/" + question.Id, question);
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpPatch]
        public async Task<IActionResult> Patch(int topicId, int testId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int topicId, int testId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int topicId, int testId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int topicId, int testId, int id)
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
                var question = await _question.Update(testId, id, value);
                if (question != null)
                {
                    return Ok(question);
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
        public async Task<IActionResult> Delete(int testId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var response = await _question.Remove(testId, id);
                if (response)
                {
                    return NoContent();
                }
                return NotFound();
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        private Question construct()
        {
            if (!parameters.ContainsKey("Question"))
                return null;
            if (!parameters["Question"].ContainsKey("QuestionText") || !parameters["Question"].ContainsKey("QuestionType"))
            {
                return null;
            }
            var question = new Question(0, parameters["Question"]["QuestionText"], 0);
            int type;
            if (int.TryParse(parameters["Question"]["QuestionType"], out type))
                question.QuestionType = type;
            return question;
        }
    }
}
