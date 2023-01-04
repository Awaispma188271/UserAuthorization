using System.ComponentModel.DataAnnotations;

namespace userAuth.Helpers
{
    public class customValidationAttribute:ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string studentName = value.ToString();
                if (studentName.Contains("Awais"))
                {
                    return ValidationResult.Success;
                }
            }
            return new ValidationResult("Wrong Name");
        }
    }
}
