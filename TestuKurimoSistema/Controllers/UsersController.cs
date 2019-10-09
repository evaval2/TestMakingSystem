using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestuKurimoSistema.DB.Entities;
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
            var list = await _user.Select();
            return Ok(list);
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _user.Select(id);
            if (user != null)
                return Ok(user);
            return NotFound(); 
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

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]User value)
        {
            if (value.Username == null || value.Password == null || value.UserRole == null)
                return BadRequest();
            var user = await _user.Update(id, value);
            if (user != null)
                return Ok(user);
            return NotFound();
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _user.Remove(id);            
            if (response)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
