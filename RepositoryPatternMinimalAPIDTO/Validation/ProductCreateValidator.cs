using FluentValidation;
using RepositoryPatternMinimalAPIDTO.Dtos;

namespace RepositoryPatternMinimalAPIDTO.Validation
{
    public class ProductCreateValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }
}
