using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
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

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, int testId, [FromBody]Question value)
        {
            if (value.QuestionText == null)
                return BadRequest();
            var question = await _question.Insert(testId, value);
            return Created("temos/"+ topicId + "/testai/"+ testId + "/klausimai/" + question.Id, question);
        }

        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int topicId, int testId, int id, [FromBody]Question value)
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

        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int testId, int id)
        {
            var response = await _question.Remove(testId, id);
            if (response)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
