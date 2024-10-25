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

        [HttpGet("{id:int}")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDto> Get([Required]int id) 
        {
            var villa = VillaStore.VillaDtos.FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
                return NotFound();

            return Ok(villa);
        }
    }
}