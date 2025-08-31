using HotelBookingAPI.CustomValidator;
using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.BookingDTOs
{
    public class RoomCostsDTO
    {
        [Required]
        public List<int> RoomIDs { get; set; }
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