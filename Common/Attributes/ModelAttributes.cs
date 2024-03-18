using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Attributes
{
    public class ModelAttributes
    {
        public class ValidEnumAttribute : ValidationAttribute
        {
            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if(value == null)
                {
                    return ValidationResult.Success;
                }

                var type = value.GetType();

                if(!(type.IsEnum && Enum.IsDefined(type, value)))
                {
                    return new ValidationResult(ErrorMessage ?? $"{value} is not a valid value for type {type.Name}");
                }

                return ValidationResult.Success;
            }
        }
    }
}
