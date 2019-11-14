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
        public UsersController()
        {
            _user = new User();
            var jsonSerializer = new JsonSerializer();
            using (var streamReader = new StreamReader(Request.Body))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                parameters = jsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(jsonTextReader);
            }
        }
        
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
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
            
            if (role == "admin")
            {
                var list = await _user.Select();
                return Ok(list);
            }
            return new ForbidResult();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
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

            if (role == "admin")
            {
                var user = await _user.Select(id);
                if (user != null)
                    return Ok(user);
                return NotFound();
            }
            return new ForbidResult();
        }
        [HttpPost("{id:int}")]
        public async Task<IActionResult> Post(int id)
        {
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
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id)
        {
            var value = construct(false);
            value.Username = id;
            if (!parameters.ContainsKey("Authorization"))
                return BadRequest();
            if (!parameters["Authorization"].ContainsKey("access_token") || !parameters["Authorization"].ContainsKey("token_type"))
                return BadRequest();
            var role = TokenValidator.Validate(parameters["Authorization"]["access_token"]);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin")
            {
                if (value == null)
                    return BadRequest();
                value.Username = id;
                var user = await _user.Update(id, value);
                if (user != null)
                    return Ok(user);
                return NotFound();
            }
            return new ForbidResult();
        }
        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            return StatusCode(405);
        }
        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
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

            if (role == "admin")
            {
                var response = await _user.Remove(id);
                if (response)
                {
                    return NoContent();
                }
                return NotFound();
            }
            return new ForbidResult();
        }
        private User construct(bool id)
        {
            if (!parameters.ContainsKey("User"))
                return null;
            if (!parameters["User"].ContainsKey("Password") || !parameters["User"].ContainsKey("UserRole"))
            {
                return null;
            }
            if (id && !parameters["User"].ContainsKey("Username"))
                return null;
            else if (!parameters["User"].ContainsKey("Username"))
                parameters["User"].Add("Username", "");
            var user = new User(parameters["User"]["Username"], parameters["User"]["Password"], parameters["User"]["UserRole"], 0, "");
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
