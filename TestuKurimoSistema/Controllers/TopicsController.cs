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
        private Error _error;
        public TopicsController()
        {
            _topic = new Topic();
            
        }
        private IActionResult checkParams(out string role)
        {
            role = "";

            string exception;
            _error = new Error(Response);
            string token = "";

            if (!Request.IsHttps)
                return Json(_error.WriteError(500, "not_secure", "", "Use https instead of http."));
            if (Request.Headers.ContainsKey("Authorization"))
                token = Request.Headers["Authorization"].ToString().Split()[1];
            else
                return Json(_error.WriteError(401, "Unauthorized", "", "The request has not been applied because it lacks valid authentication credentials for the target resource."));
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

            role = TokenValidator.Validate(token, out exception);
            if (role == null)
            {
                return Json(_error.WriteError(401, "unauthorized_client", "", "Invalid access token. " + exception));
            }
            return null;
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
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

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
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpPatch]
        public async Task<IActionResult> Patch()
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

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
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var response = await _topic.Remove(id);
                if (response)
                {
                    return NoContent(); //204
                }
                return NotFound(); //404
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
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
