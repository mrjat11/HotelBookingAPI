using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.CustomValidator
{
    public class PriceRangeValidationAttribute : ValidationAttribute
    {
        private readonly string _minPricePropertyName;
        private readonly string _maxPricePropertyName;
        public PriceRangeValidationAttribute(string minPricePropertyName, string maxPricePropertyName)
        {
            _minPricePropertyName = minPricePropertyName;
            _maxPricePropertyName = maxPricePropertyName;
            ErrorMessage = "The maximum price must be greater than or equal to the minimum price.";
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var minPriceProperty = validationContext.ObjectType.GetProperty(_minPricePropertyName);
            var maxPriceProperty = validationContext.ObjectType.GetProperty(_maxPricePropertyName);
            if (minPriceProperty == null || maxPriceProperty == null)
            {
                throw new ArgumentException("Property not found.");
            }
            var minPrice = minPriceProperty.GetValue(validationContext.ObjectInstance, null) as decimal?;
            var maxPrice = maxPriceProperty.GetValue(validationContext.ObjectInstance, null) as decimal?;
            if (!minPrice.HasValue || !maxPrice.HasValue)
            {
                return ValidationResult.Success;  // Consider how to handle nulls, potentially invalid state
            }
            if (minPrice.Value > maxPrice.Value)
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }
    }
}