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
        public QuestionsController()
        {
            _question = new Question();
            var jsonSerializer = new JsonSerializer();
            using (var streamReader = new StreamReader(Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                parameters = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonTextReader);
            }
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
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId)
        {
            if (!parameters.ContainsKey("Authorization"))
                return BadRequest();
            if (!parameters["Authorization"].ContainsKey("access_token") || !parameters["Authorization"].ContainsKey("token_type"))
                return BadRequest();
            var role = TokenValidator.Validate(parameters["Authorization"]["access_token"]);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                var value = construct();
                if (value == null)
                    return BadRequest();
                var question = await _question.Insert(testId, value);
                return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/" + question.Id, question);
            }
            return new ForbidResult();
        }
        [HttpPatch]
        public async Task<IActionResult> Patch(int topicId, int testId)
        {
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int topicId, int testId, int id)
        {
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int topicId, int testId)
        {
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int topicId, int testId, int id)
        {
            if (!parameters.ContainsKey("Authorization"))
                return BadRequest();
            if (!parameters["Authorization"].ContainsKey("access_token") || !parameters["Authorization"].ContainsKey("token_type"))
                return BadRequest();
            var role = TokenValidator.Validate(parameters["Authorization"]["access_token"]);
            if (role == "")
            {
                return Unauthorized();
            }

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
            return new ForbidResult();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int testId, int id)
        {
            if (!parameters.ContainsKey("Authorization"))
                return BadRequest();
            if (!parameters["Authorization"].ContainsKey("access_token") || !parameters["Authorization"].ContainsKey("token_type"))
                return BadRequest();
            var role = TokenValidator.Validate(parameters["Authorization"]["access_token"]);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                var response = await _question.Remove(testId, id);
                if (response)
                {
                    return NoContent();
                }
                return NotFound();
            }
            return new ForbidResult();
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
