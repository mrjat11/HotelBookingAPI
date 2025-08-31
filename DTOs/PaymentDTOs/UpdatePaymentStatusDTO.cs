using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.PaymentDTOs
{
    public class UpdatePaymentStatusDTO
    {
        [Required]
        public int PaymentID { get; set; }
        [Required]
        [RegularExpression("(Completed|Failed)", ErrorMessage = "Status must be either 'Completed' or 'Failed'.")]
        public string NewStatus { get; set; }   // 'Completed' or 'Failed'
        public string FailureReason { get; set; }
    }
}