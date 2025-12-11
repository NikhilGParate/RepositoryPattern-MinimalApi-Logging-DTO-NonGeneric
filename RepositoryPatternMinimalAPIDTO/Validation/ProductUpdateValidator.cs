using FluentValidation;
using RepositoryPatternMinimalAPIDTO.Models;

namespace RepositoryPatternMinimalAPIDTO.Validation
{
    public class ProductUpdateValidator : AbstractValidator<ProductUpdateDto>
    {
        public ProductUpdateValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        }
    }
}
