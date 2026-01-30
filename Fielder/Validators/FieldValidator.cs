using Fielder.Domain.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fielder.Validators
{
    public class FieldValidator : AbstractValidator<Field>
    {
        public FieldValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Field name cannot be empty.")
                                .MaximumLength(100).WithMessage("Field name cannot exceed 100 characters.");

            RuleFor(x => x.Value).NotEmpty().WithMessage("Field value cannot be empty.");
        }
    }
}
