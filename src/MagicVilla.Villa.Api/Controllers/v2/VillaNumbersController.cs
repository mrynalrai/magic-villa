using System.Net;
using AutoMapper;
using MagicVilla.Villa.Api.Controllers.v1;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla.Villa.Api.Controllers.v2
{
    [Route("api/v{version:apiVersion}/villanumbers")]
    [ApiController]
    [ApiVersion("2.0")]
    public class VillaNumbersController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ILogger<VillasController> _logger;
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillaNumbersController(
            ILogger<VillasController> logger, 
            IVillaNumberRepository villaNumberRepository,
            IVillaRepository villaRepository,
            IMapper mapper)
        {
            _logger = logger;
            _villaNumberRepository = villaNumberRepository;
            _villaRepository = villaRepository;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        // [MapToApiVersion("2.0")]
        public async Task<ActionResult<ApiResponse>> GetVillaNumbers()
        {
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = new string[]
            {
                "value1", "value2"
            };

            return Ok(_response);
        } 
    }
}