using AutoMapper;
using MagicVilla.Villa.Api.Models.Dtos;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<VillaModel, VillaDto>();
            CreateMap<VillaDto, VillaModel>();

            CreateMap<VillaModel, VillaCreateDto>().ReverseMap();
            CreateMap<VillaModel, VillaUpdateDto>().ReverseMap();
        }
    }
}