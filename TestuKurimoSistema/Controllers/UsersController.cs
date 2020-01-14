using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;
using Newtonsoft.Json;

namespace TestuKurimoSistema.Controllers
{ 
    [Route("vartotojai")]
    public class UsersController : Controller
    {
        private User _user;
        private Dictionary<string, Dictionary<string, string>> parameters;
        private Error _error;
        public UsersController()
        {
            _user = new User();    
        }
        private IActionResult checkParams(out string role, out string name)
        {
            role = "";
            name = "";
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

            role = TokenValidator.Validate(token, out exception, out name);
            if (role == null)
            {
                return Json(_error.WriteError(401, "unauthorized_client", "", "Invalid access token. "+exception));
            }
            return null;
        }
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            
            if (role == "admin")
            {
                var list = await _user.Select();
                return Ok(list);
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            if (name == id)
            {
                var user = await _user.Select(id);
                if (user != null)
                    return Ok(user);
                return NotFound();
            } else if (role == "admin")
            {
                var user = await _user.Select(id);
                if (user != null)
                    return Ok(user);
                return NotFound();
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post(int id)
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var value = construct(true);
            if (value == null)
                return BadRequest();
            var user = await _user.Insert(value);
            if (user != null)
                return Created("vartotojai/" + user.Username, user);
            return Conflict();
        }
        [HttpPatch]
        public async Task<IActionResult> Patch()
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id)
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        [HttpPut]
        public async Task<IActionResult> Put()
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id)
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            var value = construct(false);
            value.Username = id;

            if (role != "")
            {
                if (value == null)
                    return BadRequest();
                if (id == name)
                {
                    value.Username = id;
                    User user;
                    if (value.Token == "" && value.AverageGrade == 0)
                        user = await _user.UpdatePassword(id, value.Password);
                    else
                        user = await _user.Update(id, value);
                    if (user != null)
                        return Ok(user);
                    return NotFound();
                }
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            string role;
            string name;
            var checkParam = checkParams(out role, out name);
            if (checkParam != null)
                return checkParam;

            if (role == "admin")
            {
                if (id != name)
                {
                    var response = await _user.Remove(id);
                    if (response)
                    {
                        return NoContent();
                    }
                    return NotFound();
                }
            }
            return Json(_error.WriteError(403, "invalid_scope", "", "Permission denied."));
        }
        private User construct(bool id)
        {
            if (!parameters.ContainsKey("User"))
                return null;

            if (!parameters["User"].ContainsKey("Password"))
            {
                return null;
            }
            if (id && !parameters["User"].ContainsKey("Username"))
                return null;
            else if (!parameters["User"].ContainsKey("Username"))
                parameters["User"].Add("Username", "");
            var user = new User(parameters["User"]["Username"], parameters["User"]["Password"], "user", 0, "");
            if (parameters["User"].ContainsKey("AverageGrade")) {
                float average;
                if (float.TryParse(parameters["User"]["AverageGrade"], out average))
                    user.AverageGrade = average;
            }
            if (parameters["User"].ContainsKey("Refresh_token"))
            {                
                user.Token = parameters["User"]["Refresh_token"];
            }
            return user;
        }
    }
}
