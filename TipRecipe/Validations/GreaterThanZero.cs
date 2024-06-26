﻿using System.ComponentModel.DataAnnotations;

namespace TipRecipe.Validations
{
    public class GreaterThanZero : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Value cannot be null");
            }
            if (value is int intValue && intValue <= 0)
            {
                return new ValidationResult("Value must be greater than zero");
            }
            if (value is float floatValue && floatValue <= 0)
            {
                return new ValidationResult("Value must be greater than zero");
            }
            if (value is double doubleValue && doubleValue <= 0)
            {
                return new ValidationResult("Value must be greater than zero");
            }
            return ValidationResult.Success;
        }
    }
}
