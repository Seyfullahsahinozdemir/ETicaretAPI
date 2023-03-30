using ETicaretAPI.Application.ViewModels.Products;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETicaretAPI.Application.Validators.Products
{
    public class CreateProductValidator: AbstractValidator<VM_Create_Product>
    {
        public CreateProductValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Required product name.")
                .MaximumLength(150)
                .MinimumLength(5)
                    .WithMessage("product name must be between 5 - 150 characters.");

            RuleFor(p => p.Stock)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Required stock")
                .Must(s => s >= 0)
                    .WithMessage("Stock must be nonnegative");

            RuleFor(p => p.Price)
                .NotEmpty()
                .NotNull()
                    .WithMessage("Required price")
                .Must(s => s >= 0)
                    .WithMessage("Price must be nonnegative");
        }
    }
}
