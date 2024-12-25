using AutoMapper;
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
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> Get() 
        {
            _logger.LogInformation("Getting all villas");
            IEnumerable<VillaModel> villas = await _villaRepository.GetAllAsync();
            return Ok(_mapper.Map<List<VillaDto>>(villas));
        }

        [HttpGet("{id:int}", Name = "Get")]
        // [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaDto))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VillaDto>> Get(int id) 
        {
            if (id == 0)
            {
                _logger.LogError($"Get villa error with id: {id}");
                return BadRequest();
            }
            var villa = await _villaRepository.GetAsync(villa => villa.Id == id);   
            if (villa == null)
                return NotFound();

            return Ok(_mapper.Map<VillaDto>(villa));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Create([FromBody] VillaCreateDto createDto) 
        {
            if (createDto == null) 
            {
                return BadRequest();
            }
            if (await _villaRepository.GetAsync(villa => villa.Name.ToLower() == createDto.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", $"Villa with the name {createDto.Name} already exists.");
                return BadRequest(ModelState);
            }

            VillaModel model = _mapper.Map<VillaModel>(createDto);

            await _villaRepository.CreateAsync(model);

            return CreatedAtRoute("Get", new { id = model.Id}, model);
        }

        [HttpDelete("{id:int}", Name = "Delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
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
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "Update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody]VillaUpdateDto updateDto)
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

            return NoContent();
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