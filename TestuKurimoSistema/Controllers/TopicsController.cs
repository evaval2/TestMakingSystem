using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
namespace TestuKurimoSistema.Controllers
{
    [Route("temos")]
    public class TopicsController : Controller
    {
        private Topic _topic;
        public TopicsController()
        {
            _topic = new Topic();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var list = await _topic.Select();
            return Ok(list); //200
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var topic = await _topic.Select(id);
            if (topic != null)
                return Ok(topic); //200
            return NotFound(); //404
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post()
        {
            return StatusCode(405);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Topic value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.TopicName == null)
                {
                    return BadRequest();
                }
                var topic = await _topic.Insert(value);
                if (topic != null)
                    return Created("temos/" + topic.Id, topic); //201
                return Conflict(); //409
            }
            return new ForbidResult();
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
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Topic value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                if (value.TopicName == null)
                {
                    return BadRequest();
                }
                var topic = await _topic.Update(id, value);
                if (topic != null)
                {
                    if (topic.Id >= 0)
                        return Ok(topic); //200
                    return Conflict();
                }
                return NotFound(); //404
            }
            return new ForbidResult();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return StatusCode(405);
        }
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin" || role == "user")
            {
                var response = await _topic.Remove(id);
                if (response)
                {
                    return NoContent(); //204
                }
                return NotFound(); //404
            }
            return new ForbidResult();
        }
    }
}
