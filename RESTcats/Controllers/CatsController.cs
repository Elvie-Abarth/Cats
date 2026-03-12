using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RESTcats.Models;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RESTcats.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private CatsRepositoryList _repo;

        public CatsController(CatsRepositoryList repo)
        {
            _repo = repo;
        }

        // GET: api/<CatsController>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        
        [Authorize(Roles = "Admin")]
        public ActionResult<IEnumerable<Cat>> Get(
            [FromQuery] int minimumweight,
            [FromQuery] int? maximumweight,
            [FromQuery] string? nameFilter)
        {
            try
            {
                IEnumerable<Cat> result = _repo.GetAllCats(minimumweight
                    , maximumweight, nameFilter);
                if (result == null || result.Count() == 0)
                {
                    return NoContent();
                }
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<CatsController>/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        [Authorize(Roles = "User")]
        public ActionResult<Cat> Get(int id)
        {
            Cat? cat = _repo.GetCatById(id);
            if (cat == null)
            {
                return NotFound();
            }
            return Ok(cat);
        }

        // POST api/<CatsController>
        [HttpPost]
        public Cat Post([FromBody] Cat newCat)
        {
            return _repo.AddCat(newCat);
        }

        // PUT api/<CatsController>/5
        [HttpPut("{id}")]
        public Cat? Put(int id, [FromBody] Cat value)
        {
            return _repo.UpdateCat(id, value);
        }

        // DELETE api/<CatsController>/5
        [HttpDelete("{id}")]
        public Cat? Delete(int id)
        {
            return _repo.RemoveCat(id);
        }

        [HttpOptions]
        public void Options()
        {
        }

    }
}
