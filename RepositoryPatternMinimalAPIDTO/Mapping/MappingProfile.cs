using AutoMapper;
using RepositoryPatternMinimalAPIDTO.Dtos;
using RepositoryPatternMinimalAPIDTO.Models;

namespace RepositoryPatternMinimalAPIDTO.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProductCreateDto, Product>();
            CreateMap<ProductUpdateDto, Product>();
            CreateMap<Product, ProductReadDto>();
        }
    }
}
