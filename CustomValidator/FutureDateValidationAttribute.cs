using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.CustomValidator
{
    public class FutureDateValidationAttribute : ValidationAttribute
    {
        public FutureDateValidationAttribute()
        {
            ErrorMessage = "The date must be in the future.";
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var date = value as DateTime?;
            // Check if the date is in the future
            if (!date.HasValue || date.Value.Date <= DateTime.Today)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}