using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
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

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId, int questionId, [FromBody]Answer value)
        {
            if (value.AnswerText == null)
                return BadRequest();
            var answer = await _answer.Insert(questionId, value);
            return Created("temos/" + topicId + "/testai/" + testId + "/klausimai/" 
                                    + questionId + "/atsakymai/" + answer.Id, answer);
        }

        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int questionId, int id, [FromBody]Answer value)
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

        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int questionId, int id)
        {
            var response = await _answer.Remove(questionId, id);
            if (response)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
