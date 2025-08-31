using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class UpdateRefundStatusRequestDTO
    {
        [Required(ErrorMessage = "RefundID is required.")]
        public int RefundID { get; set; }

        [Required(ErrorMessage = "NewRefundStatus is required.")]
        [StringLength(50, ErrorMessage = "NewRefundStatus length cannot exceed 50 characters.")]
        public string NewRefundStatus { get; set; }
    }
}