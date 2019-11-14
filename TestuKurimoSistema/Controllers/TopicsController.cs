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
    [Route("temos")]
    public class TopicsController : Controller
    {
        private Topic _topic;
        private Dictionary<string, Dictionary<string, string>> parameters;
        public TopicsController()
        {
            _topic = new Topic();
            var jsonSerializer = new JsonSerializer();
            using (var streamReader = new StreamReader(Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                parameters = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonTextReader);
            }
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
        public async Task<IActionResult> Post(int id)
        {
            return StatusCode(405);
        }
        [HttpPost]
        public async Task<IActionResult> Post()
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
        public async Task<IActionResult> Put(int id)
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
                {
                    return BadRequest();
                }
                value.Id = id;
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
                var response = await _topic.Remove(id);
                if (response)
                {
                    return NoContent(); //204
                }
                return NotFound(); //404
            }
            return new ForbidResult();
        }
        private Topic construct()
        {
            if (!parameters.ContainsKey("Topic"))
                return null;
            if (!parameters["Topic"].ContainsKey("TopicName"))
            {
                return null;
            }
            var topic = new Topic(0, parameters["Topic"]["TopicName"]);            
            return topic;
        }
    }
}
