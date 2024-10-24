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
        public IEnumerable<VillaDto> Get() 
        {
            return VillaStore.VillaDtos;
        }

        [HttpGet("{id:int}")]
        public VillaDto Get([Required]int id) 
        {
            return VillaStore.VillaDtos.FirstOrDefault(villa => villa.Id == id);
        }
    }
}