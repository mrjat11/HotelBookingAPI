using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.CustomValidator
{
    public class DateGreaterThanValidationAttribute : ValidationAttribute
    {
        private readonly string _comparisonPropertyName;
        public DateGreaterThanValidationAttribute(string comparisonPropertyName)
        {
            _comparisonPropertyName = comparisonPropertyName;
            ErrorMessage = "The date must be greater than the comparison date.";
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentDate = value as DateTime?;
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonPropertyName);
            var comparisonDate = comparisonProperty?.GetValue(validationContext.ObjectInstance, null) as DateTime?;
            // Check if the current date is greater than the comparison date
            if (currentDate.HasValue && comparisonDate.HasValue && currentDate.Value <= comparisonDate.Value)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}