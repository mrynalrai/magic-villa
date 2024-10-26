using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MagicVilla.Villa.Api.Controllers
{
    // [Route("[controller]")]
    [Route("api/villas")]
    [ApiController]
    public class VillasController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDto>> Get() 
        {
            return Ok(VillaStore.VillaDtos);
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
                return BadRequest();
            }
            var villa = VillaStore.VillaDtos.FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(villa);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
            if (VillaStore.VillaDtos.Find(villa => villa.Name.Equals(villaDto.Name, StringComparison.InvariantCultureIgnoreCase)) != null) 
            {
                ModelState.AddModelError("CustomError", $"Villa with the name {villaDto.Name} already exists.");
                return BadRequest(ModelState);
            }

            villaDto.Id = VillaStore.VillaDtos.OrderByDescending(villaDto => villaDto.Id).FirstOrDefault().Id + 1;
            VillaStore.VillaDtos.Add(villaDto);

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
            var villa = VillaStore.VillaDtos.Find(villa => villa.Id == id);
            if (villa == null) 
            {
                return NotFound();
            }
            VillaStore.VillaDtos.Remove(villa);
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

            var villa = VillaStore.VillaDtos.Find(villa => villa.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            villa.Name = villaDto.Name;
            villa.Occupancy = villaDto.Occupancy;
            villa.Sqft = villaDto.Sqft;

            return NoContent();
        }
    }
}