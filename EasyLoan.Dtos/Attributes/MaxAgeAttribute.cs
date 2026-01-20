using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Attributes
{
    public class MaxAgeAttribute : ValidationAttribute
    {
        private readonly int _maxAge;

        public MaxAgeAttribute(int maxAge)
        {
            _maxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateOfBirth)
            {
                var age = DateTime.Today.Year - dateOfBirth.Year;
                if (dateOfBirth > DateTime.Today.AddYears(-age))
                    age--;

                if (age > _maxAge)
                    return new ValidationResult($"Age cannot exceed {_maxAge} years.");

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date of birth.");
        }
    }

}
