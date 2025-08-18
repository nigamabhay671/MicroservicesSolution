using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace CatalogService.Application.Products
{
    public class CreateProductValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        }
    }
}
