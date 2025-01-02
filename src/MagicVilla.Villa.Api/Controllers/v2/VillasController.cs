using System.Net;
using System.Text.Json;
using AutoMapper;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using static MagicVilla.Villa.Api.Common.Constants;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Controllers.v2
{
    [Route("api/v{version:apiVersion}/villas")]
    [ApiController]
    [ApiVersion("2.0")]
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
        [Authorize]
        // [ResponseCache(Duration = 30)]  // 30 seconds
        [ResponseCache(CacheProfileName = "Default30")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get(
            [FromQuery(Name = "filterOccupancy")]int? occupancy,
            [FromQuery]string? search,
            [FromQuery]int pageSize = 10,
            [FromQuery]int pageNumber = 1
        ) 
        {
            _logger.LogInformation("Getting all villas");
            try 
            {
                IEnumerable<VillaModel> villas;
                if (pageNumber < 0 || pageSize < 0)
                {
                    _logger.LogError($"Inavlid Page size or Page number");
                    _response.ErrorMessages = new List<string>()
                    {
                        $"Inavlid Page size or Page number"
                    };
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                if (occupancy > 0)
                {
                    villas = await _villaRepository.GetAllAsync(villa => villa.Occupancy == occupancy, pageSize: pageSize, pageNumber: pageNumber);
                }
                else
                {
                    villas = await _villaRepository.GetAllAsync(pageSize: pageSize, pageNumber: pageNumber);
                }
                if (!string.IsNullOrWhiteSpace(search))
                {
                    villas = villas.Where(villa => villa.Name.ToUpper().Contains(search.ToUpper()));
                }
                 
                Pagination pagination = new Pagination
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));
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
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpGet("{id:int}", Name = "GetVilla")]
        [Authorize]
        // [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)] 
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Create([FromForm] VillaCreateDto createDto) 
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
                    // ModelState.AddModelError("CustomError", $"Villa with the name {createDto.Name} already exists.");
                    // return BadRequest(ModelState);
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = [
                        $"Villa with the name {createDto.Name} already exists."
                    ];
                    return BadRequest(_response);
                }

                VillaModel villa = _mapper.Map<VillaModel>(createDto);

                if (createDto.Image != null)
                {
                    string fileName = villa.Id + Path.GetExtension(createDto.Image.FileName);
                    string filePath = Path.Combine("wwwroot", "ProductImage", fileName);

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    FileInfo file = new FileInfo(directoryLocation);

                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        createDto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    villa.ImageUrl = baseUrl+"/ProductImage/"+fileName;
                    villa.ImageLocalPath = filePath;
                }
                else
                {
                    villa.ImageUrl = DefaultImageUrl;
                }

                await _villaRepository.CreateAsync(villa);

                _response.Result = _mapper.Map<VillaDto>(villa);
                _response.StatusCode = HttpStatusCode.Created;
                return CreatedAtRoute("GetVilla", new { id = villa.Id}, villa);
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

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            try
            {
                if (id == 0)
                {
                    // return BadRequest();
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var villa = await _villaRepository.GetAsync(villa => villa.Id == id);
                if (villa == null) 
                {
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(villa.ImageLocalPath))
                {
                    var oldFilePathDirectoy = Path.Combine(Directory.GetCurrentDirectory(), villa.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectoy);

                    if (file.Exists)
                    {
                        file.Delete();
                    }
                }

                await _villaRepository.RemoveAsync(villa);
                
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

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromForm]VillaUpdateDto updateDto)
        {
            try
            {
                if (updateDto == null || updateDto.Id != id)
                {
                    // return BadRequest();
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villa = await _villaRepository.GetAsync(villa => villa.Id == id);
                if (villa == null)
                {
                    // return NotFound();
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _mapper.Map(updateDto, villa);

                if (updateDto.Image != null)
                {
                    if (!string.IsNullOrWhiteSpace(villa.ImageLocalPath))
                    {
                        var oldFilePathDirectoy = Path.Combine(Directory.GetCurrentDirectory(), villa.ImageLocalPath);
                        FileInfo file = new FileInfo(oldFilePathDirectoy);

                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }

                    string fileName = villa.Id + Path.GetExtension(updateDto.Image.FileName);
                    string filePath = Path.Combine("wwwroot", "ProductImage", fileName);

                    var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), filePath);

                    using (var fileStream = new FileStream(directoryLocation, FileMode.Create))
                    {
                        updateDto.Image.CopyTo(fileStream);
                    }

                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                    villa.ImageUrl = baseUrl+"/ProductImage/"+fileName;
                    villa.ImageLocalPath = filePath;
                }
                else
                {
                    villa.ImageUrl = DefaultImageUrl;
                }

                await _villaRepository.UpdateAsync(villa);

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

        [HttpPatch("{id:int}", Name = "PatchVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePartial(int id, [FromBody]JsonPatchDocument<VillaUpdateDto> villaPatchDoc)
        {
            if (villaPatchDoc == null || id == 0)
            {
                // return BadRequest();
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
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
                // return BadRequest(ModelState);
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            VillaModel model = _mapper.Map<VillaModel>(villaDto);

            await _villaRepository.UpdateAsync(model);

            return NoContent();
        }
    }
}