using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Validations
{
    public class LowerThanTen : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Value cannot be null");
            }
            if (value is int intValue && intValue > 10)
            {
                return new ValidationResult("Value must be lower than ten");
            }
            if (value is float floatValue && floatValue > 10)
            {
                return new ValidationResult("Value must be lower than ten");
            }
            if (value is double doubleValue && doubleValue > 10)
            {
                return new ValidationResult("Value must be lower than ten");
            }
            return ValidationResult.Success;
        }
    }
}
