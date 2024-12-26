using System.Net;
using AutoMapper;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Controllers
{
    // [Route("[controller]")]
    [Route("api/villas")]
    [ApiController]
    public class VillasController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ILogger<VillasController> _logger;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillasController(
            ILogger<VillasController> logger, 
            IVillaRepository villaRepository,
            IMapper mapper)
        {
            _logger = logger;
            _villaRepository = villaRepository;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> Get() 
        {
            _logger.LogInformation("Getting all villas");
            try 
            {
                IEnumerable<VillaModel> villas = await _villaRepository.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaDto>>(villas);
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
                return _response;
            }
        }

        [HttpGet("{id:int}", Name = "Get")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                var villa = await _villaRepository.GetAsync(villa => villa.Id == id);   
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaDto>(villa);
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
                return _response;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] VillaCreateDto createDto) 
        {
            try
            {
                if (createDto == null) 
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if (await _villaRepository.GetAsync(villa => villa.Name.ToLower() == createDto.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("CustomError", $"Villa with the name {createDto.Name} already exists.");
                    return BadRequest(ModelState);
                }

                VillaModel villa = _mapper.Map<VillaModel>(createDto);

                await _villaRepository.CreateAsync(villa);

                _response.Result = _mapper.Map<VillaDto>(villa);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("Get", new { id = villa.Id}, villa);
            }
            catch (Exception ex) 
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() 
                {
                    ex.ToString()
                };
                return _response;
            }
        }

        [HttpDelete("{id:int}", Name = "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }
                var villa = await _villaRepository.GetAsync(villa => villa.Id == id);
                if (villa == null) 
                {
                    return NotFound();
                }
                await _villaRepository.RemoveAsync(villa);
                
                _response.StatusCode = HttpStatusCode.Created;
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
                return _response;
            }
        }

        [HttpPut("{id:int}", Name = "Update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody]VillaUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || updateDto.Id != id)
                {
                    return BadRequest();
                }

                var villa = await _villaRepository.GetAsync(villa => villa.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }

                _mapper.Map(updateDto, villa);

                await _villaRepository.UpdateAsync(villa);

                _response.StatusCode = HttpStatusCode.NoContent;
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
                return _response;
            }
        }

        [HttpPatch("{id:int}", Name = "Patch")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody]JsonPatchDocument<VillaUpdateDto> villaPatchDoc)
        {
            if (villaPatchDoc == null || id == 0)
            {
                return BadRequest();
            }

            var villa = await _villaRepository.GetAsync(villa => villa.Id == id, false);
            if (villa == null)
            {
                return NotFound();
            }

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            villaPatchDoc.ApplyTo(villaDto, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VillaModel model = _mapper.Map<VillaModel>(villaDto);

            await _villaRepository.UpdateAsync(model);

            return NoContent();
        }
    }
}