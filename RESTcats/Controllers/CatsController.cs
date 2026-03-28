using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TextTemplating;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using RESTcats.Models;
using System.Linq;

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
        //[HttpGet]
        //public IEnumerable<Cat> Get()
        //{
        //    return _repo.GetAllCats();
        //}

        // GET: api/<CatsController>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        [HttpGet("get")]
        public ActionResult<IEnumerable<Cat>> Get([FromQuery] int? minimumweight, [FromQuery] int? maximumweight)
        {
            IEnumerable<Cat> result = _repo.GetAllCats(minimumweight, maximumweight);       
                
                if (result == null || !result.Any())
                {
                    return NoContent();
                }

                return Ok(result);
        }

        // GET: api/<CatsController>/weight/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpGet("weight/{minWeight}")]
        public ActionResult<IEnumerable<Cat>> FilterByMinWeight(int minWeight)
        {
            var cats = _repo.GetAllCats(minWeight);

            if (cats == null)
            {
                return NotFound();
            }

            if (!cats.Any())
            {
                return NoContent();
            }

            return Ok(cats);
        }

        // GET: api/<CatsController>/weight?minWeight=5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpGet("weight")]
        public ActionResult<IEnumerable<Cat>> GetByMinWeight([FromQuery] int minWeight)
        {
            var cats = _repo.GetAllCats(minWeight);

            if (!cats.Any())
            {
                return NoContent();
            }

            return Ok(cats);
        }


        // GET api/<CatsController>/5
        //[HttpGet("{id}")]
        //public Cat? Get(int id)
        //{
        //    return _repo.GetCatById(id);
        //}

        // GET api/<CatsController>/5
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpGet("{id}")]
        public ActionResult<Cat> Get(int id)
        {
            Cat? cat = _repo.GetCatById(id);
            
            if (cat == null)
            {
                return NotFound();
            }         
            
            return Ok(cat);            
        }

        // GET api/items
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "User")]
        [HttpGet("getall")]
        public ActionResult<IEnumerable<Cat>> GetAllCats()
        {
            var cats = _repo.GetAllCats();

            if (cats == null)
            {
                return NotFound();
            }

            if (!cats.Any())
            {
                return NoContent();
            }

            return Ok(cats);
        }

        // GET api/items/search?substring=abc:
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "User")]
        [HttpGet("search")]
        public ActionResult<IEnumerable<Cat>> GetBySubstring([FromQuery] string substring)
        {
            var cats = _repo.GetAllCats(substring);

            if (cats == null)
            {
                return NotFound();
            }

            if (!cats.Any())
            {
                return NoContent();
            }

            return Ok(cats);
        }

        // GET api/items/name/abc
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "User")]
        [HttpGet("name/{substring}")]
        public ActionResult<IEnumerable<Cat>> GetByName(string substring)
        {
            var cats = _repo.GetAllCats(substring);

            if (cats == null)
            {
                return NotFound();
            }

            if (!cats.Any())
            {
                return NoContent();
            }

            return Ok(cats);
        }


        //// POST api/<CatsController>
        //[HttpPost]
        //public Cat Post([FromBody] Cat newCat)
        //{
        //    return _repo.AddCat(newCat);
        //}

        // POST api/<CatsController>                
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult<Cat> Post([FromBody] Cat newItem)
        {
            try
            {
                _repo.AddCat(newItem);
                return Created($"api/items/{newItem.Id}", newItem);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST api/cats        
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public ActionResult<Cat> Create(Cat newCat)
        {
            try
            {
                // Validate input (example)
                if (string.IsNullOrWhiteSpace(newCat.Name))
                {
                    return BadRequest("Name cannot be empty");
                }

                if (newCat.Weight <= 0)
                {
                    return BadRequest("Weight must be positive");
                }

                // Save to repo
                Cat created = _repo.AddCat(newCat);

                // Return 201 + Location header
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                // Return 400 with your exception message
                return BadRequest(ex.Message);
            }
        }


        // PUT api/<CatsController>/5        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpPut("{id}")]
        public ActionResult<Cat?> Put(int id, [FromBody] Cat value)
        {
            // Validate id (int is non-nullable; check for an invalid value instead)
            if (id <= 0)
            {
                return BadRequest("Id must be a positive integer");
            }

            // Attempt update
            Cat? updated = _repo.UpdateCat(id, value);

            if (updated == null)
            {
                return NotFound();
            }

            return Ok(updated);
        }


        // DELETE api/<CatsController>/5        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public ActionResult<Cat?> Delete(int id)
        {
            // Validate id
            if (id <= 0)
            {
                return BadRequest("Id must be a positive integer");
            }

            // Check existence
            var existing = _repo.GetCatById(id);
            if (existing == null)
            {
                return NotFound();
            }

            // Remove and return removed entity
            var removed = _repo.RemoveCat(id);
            return Ok(removed);
        }

        [HttpOptions]
        public void Options()
        {
        }

    }
}
