using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.PaymentDTOs
{
    public class ProcessPaymentDTO
    {
        [Required]
        public int ReservationID { get; set; }
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than zero.")]
        public decimal TotalAmount { get; set; }
        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; }
    }
}