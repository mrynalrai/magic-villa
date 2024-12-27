using System.Net;
using AutoMapper;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla.Villa.Api.Controllers
{
    [Route("api/villanumbers")]
    [ApiController]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get() 
        {
            _logger.LogInformation("Getting all Villa Numbers");
            try 
            {
                IEnumerable<VillaNumber> villaNums = await _villaNumberRepository.GetAllAsync(includeProperties: "Villa");
                _response.Result = _mapper.Map<List<VillaNumber>>(villaNums);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get(int id) 
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError($"Get villa error with id: {id}");
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villaNum = await _villaNumberRepository.GetAsync(villaNum => villaNum.VillaNum == id);   
                if (villaNum == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaNumberDto>(villaNum);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] VillaNumberCreateDto createDto) 
        {
            try
            {
                if (createDto == null) 
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if (await _villaNumberRepository.GetAsync(villaNum => villaNum.VillaNum == createDto.VillaNum) != null)
                {
                    // ModelState.AddModelError("ErrorMessages", $"VillaNumber with the villa number {createDto.VillaNum} already exists.");
                    // return BadRequest(ModelState);
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = [
                        $"VillaNumber with the villa number {createDto.VillaNum} already exists."
                    ];
                    return BadRequest(_response);
                }
                if (await _villaRepository.GetAsync(villa => villa.Id == createDto.VillaId) == null)
                {
                    // ModelState.AddModelError("ErrorMessages", $"Villa ID {createDto.VillaId} is invalid.");
                    // return BadRequest(ModelState);
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = [
                        $"Villa ID {createDto.VillaId} is invalid."
                    ];
                    return BadRequest(_response);
                }

                VillaNumber villaNum = _mapper.Map<VillaNumber>(createDto);

                await _villaNumberRepository.CreateAsync(villaNum);

                _response.Result = _mapper.Map<VillaNumberDto>(villaNum);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = villaNum.VillaNum}, _response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                    // return BadRequest();
                }
                var villaNum = await _villaNumberRepository.GetAsync(villa => villa.VillaNum == id);
                if (villaNum == null) 
                {
                    return NotFound();
                }
                await _villaNumberRepository.RemoveAsync(villaNum);
                
                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody]VillaNumberUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || updateDto.VillaNum != id)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                    // return BadRequest();
                }

                var villaNum = await _villaNumberRepository.GetAsync(villa => villa.VillaNum == id);
                if (villaNum == null)
                {
                    return NotFound();
                }
                
                if (await _villaRepository.GetAsync(villa => villa.Id == updateDto.VillaId) == null)
                {
                    // ModelState.AddModelError("ErrorMessages", $"Villa ID {updateDto.VillaId} is invalid.");
                    // return BadRequest(_response);
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = [
                        $"Villa ID {updateDto.VillaId} is invalid."
                    ];
                    return BadRequest(_response);
                }

                _mapper.Map(updateDto, villaNum);

                await _villaNumberRepository.UpdateAsync(villaNum);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }
    }
}