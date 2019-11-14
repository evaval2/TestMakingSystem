using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
namespace TestuKurimoSistema.Controllers
{
    [Route("temos/{topicId:int}/testai/{testId:int}/klausimai/{questionId:int}/atsakymai")]
    public class AnswersController : Controller
    {
        private Answer _answer;
        public AnswersController()
        {
            _answer = new Answer();
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
        public async Task<IActionResult> Post()
        {
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId, int questionId, [FromBody]Answer value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.AnswerText == null)
                    return BadRequest();
                var answer = await _answer.Insert(questionId, value);
                return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/"
                                        + questionId + "/atsakymai/" + answer.Id, answer);
            }
            return Unauthorized();
        }
        [HttpPatch]
        public async Task<IActionResult> Patch()
        {
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id)
        {
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int questionId, int id, [FromBody]Answer value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.AnswerText == null)
                    return BadRequest();
                var answer = await _answer.Update(questionId, id, value);
                if (answer != null)
                {
                    return Ok(answer);
                }
                return NotFound();
            }
            return Unauthorized();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int questionId, int id)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                var response = await _answer.Remove(questionId, id);
                if (response)
                {
                    return NoContent();
                }
                return NotFound();
            }
            return Unauthorized();
        }
    }
}
