using HotelBookingAPI.CustomValidator;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.BookingDTOs
{
    public class CreateReservationDTO
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one room ID must be provided.")]
        public List<int> RoomIDs { get; set; }  // Room IDs for the reservation
        
        [Required]
        [DataType(DataType.Date)]
        [FutureDateValidation(ErrorMessage = "Check-in date must be in the future.")]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        [FutureDateValidation(ErrorMessage = "Check-out date must be in the future.")]
        [DateGreaterThanValidation("CheckInDate", ErrorMessage = "Check-out date must be after check-in date.")]
        public DateTime CheckOutDate { get; set; }
    }
}