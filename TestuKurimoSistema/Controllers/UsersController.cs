using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using TestuKurimoSistema.DB.Entities;
using TestuKurimoSistema.OAuth2;

namespace TestuKurimoSistema.Controllers
{ 
    [Route("vartotojai")]
    public class UsersController : Controller
    {
        private User _user;
        public UsersController()
        {
            _user = new User();
        }
        
        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var role = TokenValidator.Validate(Request);
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
            var role = TokenValidator.Validate(Request);
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
        public async Task<IActionResult> Post()
        {
            return StatusCode(405);
        }
        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]User value)
        {
            if (value.Username == null || value.Password == null || value.UserRole == null)
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
        public async Task<IActionResult> Put(string id, [FromBody]User value)
        {
            var role = TokenValidator.Validate(Request);
            if (role == "")
            {
                return Unauthorized();
            }

            if (role == "admin")
            {
                if (value.Username == null || value.Password == null || value.UserRole == null)
                    return BadRequest();
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
            var role = TokenValidator.Validate(Request);
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
        
    }
}
