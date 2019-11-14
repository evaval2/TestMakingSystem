using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
namespace TestuKurimoSistema.Controllers
{
    [Route("temos/{topicId:int}/testai")]
    public class TestsController : Controller
    {
        private Test _test;
        public TestsController()
        {
            _test = new Test();
        }
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(int topicId)
        {
            var list = await _test.Select(topicId);
            return Ok(list); //200
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int topicId, int id)
        {
            var test = await _test.Select(topicId, id);
            if (test != null)
                return Ok(test); //200
            return NotFound(); //404
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post()
        {
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId, [FromBody]Test value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.TestName == null)
                {
                    return BadRequest();
                }
                var test = await _test.Insert(topicId, value);
                return Created("temos/" + topicId + "/testai/" + test.Id, test); //201
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
        public async Task<IActionResult> Put(int topicId, int id, [FromBody]Test value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.TestName == null)
                {
                    return BadRequest();
                }
                var test = await _test.Update(topicId, id, value);
                if (test != null)
                {
                    return Ok(test); //200
                }
                return NotFound(); //404
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
        public async Task<IActionResult> Delete(int topicId, int id)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                var response = await _test.Remove(topicId, id);
                if (response)
                {
                    return NoContent(); //204
                }
                return NotFound(); //404
            }
            return Unauthorized();
        }
    }
}
