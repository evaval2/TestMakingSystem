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
    [Route("temos/{topicId:int}/testai")]
    public class TestsController : Controller
    {
        private Test _test;
        private Dictionary<string, Dictionary<string, string>> parameters;
        private Error _error;
        public TestsController()
        {
            _test = new Test();
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
        public async Task<IActionResult> Post(int topicId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(int topicId)
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
                var test = await _test.Insert(topicId, value);
                return Created("temos/" + topicId + "/testai/" + test.Id, test); //201
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpPatch]
        public async Task<IActionResult> Patch(int topicId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int topicId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put(int topicId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int topicId, int id)
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
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(int topicId)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int topicId, int id)
        {
            string role;
            var checkParam = checkParams(out role);
            if (checkParam != null)
                return checkParam;

            if (role == "admin" || role == "user")
            {
                var response = await _test.Remove(topicId, id);
                if (response)
                {
                    return NoContent(); //204
                }
                return NotFound(); //404
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        private Test construct()
        {
            if (!parameters.ContainsKey("Test"))
                return null;
            if (!parameters["Test"].ContainsKey("TestName"))
            {
                return null;
            }
            var test = new Test(0, parameters["Test"]["TestName"], 0, 0);
            if (parameters["Test"].ContainsKey("AverageGrade"))
            {
                float average;
                if (float.TryParse(parameters["Test"]["AverageGrade"], out average))
                    test.AverageGrade = average;
            }
            if (parameters["Test"].ContainsKey("TestTime"))
            {
                int time;
                if (int.TryParse(parameters["Test"]["TestTime"], out time))
                    test.TestTime = time;
            }
            return test;
        }
    }
}
