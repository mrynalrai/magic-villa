using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VillaItem = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Controllers
{
    // [Route("[controller]")]
    [Route("api/villas")]
    [ApiController]
    public class VillasController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<VillaItem> Get() 
        {
            var results = new List<VillaItem> {
                new() {
                    Id = 1,
                    Name = "Beach View"
                },
                new() {
                    Id = 2,
                    Name = "City View"
                } 
            };

            return results;
        }
    }
}