using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
namespace TestuKurimoSistema.Controllers
{
    [Route("temos/{topicId:int}/testai/{testId:int}/klausimai")]
    public class QuestionsController : Controller
    {
        private Question _question;
        public QuestionsController()
        {
            _question = new Question();
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
        public async Task<IActionResult> Post()
        {
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId, [FromBody]Question value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.QuestionText == null)
                    return BadRequest();
                var question = await _question.Insert(testId, value);
                return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/" + question.Id, question);
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
        public async Task<IActionResult> Put(int topicId, int testId, int id, [FromBody]Question value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.QuestionText == null)
                    return BadRequest();
                var question = await _question.Update(testId, id, value);
                if (question != null)
                {
                    return Ok(question);
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
        public async Task<IActionResult> Delete(int testId, int id)
        {
            var role = TokenValidator.Validate(Request);
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
            return Unauthorized();
        }
    }
}
