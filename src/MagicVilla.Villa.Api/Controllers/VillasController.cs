using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Controllers
{
    // [Route("[controller]")]
    [Route("api/villas")]
    [ApiController]
    public class VillasController : ControllerBase
    {
        private readonly ILogger<VillasController> _logger;
        private readonly ApplicationDbContext _dbContext;

        public VillasController(ILogger<VillasController> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDto>> Get() 
        {
            _logger.LogInformation("Getting all villas");
            return Ok(_dbContext.Villas.ToList());
        }

        [HttpGet("{id:int}", Name = "Get")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<VillaDto> Get(int id) 
        {
            if (id == 0)
            {
                _logger.LogError($"Get villa error with id: {id}");
                return BadRequest();
            }
            var villa = _dbContext.Villas.FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Create([FromBody] VillaDto villaDto) 
        {
            if (villaDto == null) 
            {
                return BadRequest();
            }
            if (villaDto.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            if (_dbContext.Villas.FirstOrDefault(villa => villa.Name.ToLower() == villaDto.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", $"Villa with the name {villaDto.Name} already exists.");
                return BadRequest(ModelState);
            }

            VillaModel model = new() {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft
            };

            _dbContext.Villas.Add(model);
            _dbContext.SaveChanges();

            return CreatedAtRoute("Get", new { id = villaDto.Id}, villaDto);
        }

        [HttpDelete("{id:int}", Name = "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            var villa = _dbContext.Villas.FirstOrDefault(villa => villa.Id == id);
            if (villa == null) 
            {
                return NotFound();
            }
            _dbContext.Villas.Remove(villa);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "Update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Update(int id, [FromBody]VillaDto villaDto)
        {
            if (villaDto == null || villaDto.Id != id)
            {
                return BadRequest();
            }

            var villa = _dbContext.Villas.FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            /*
            villa.Name = villaDto.Name;
            villa.Occupancy = villaDto.Occupancy;
            villa.Sqft = villaDto.Sqft;
            */

            // Update the existing entity
            villa.Amenity = villaDto.Amenity;
            villa.Details = villaDto.Details;
            villa.ImageUrl = villaDto.ImageUrl;
            villa.Name = villaDto.Name;
            villa.Occupancy = villaDto.Occupancy;
            villa.Rate = villaDto.Rate;
            villa.Sqft = villaDto.Sqft;

            _dbContext.SaveChanges();

            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "Patch")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdatePartial(int id, [FromBody]JsonPatchDocument<VillaDto> villaPatchDoc)
        {
            if (villaPatchDoc == null || id == 0)
            {
                return BadRequest();
            }

            var villa = _dbContext.Villas.AsNoTracking().FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            VillaDto villaDto = new() {
                Amenity = villa.Amenity,
                Details = villa.Details,
                Id = villa.Id,
                ImageUrl = villa.ImageUrl,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Rate = villa.Rate,
                Sqft = villa.Sqft
            };

            villaPatchDoc.ApplyTo(villaDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VillaModel model = new() {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft
            };

            _dbContext.Update(model);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}