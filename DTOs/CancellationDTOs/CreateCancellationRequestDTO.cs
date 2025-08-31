using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class CreateCancellationRequestDTO
    {
        [Required(ErrorMessage = "UserID is required.")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "ReservationID is required.")]
        public int ReservationID { get; set; }

        [Required(ErrorMessage = "RoomsCancelled list cannot be empty.")]
        [MinLength(1, ErrorMessage = "At least one room must be cancelled.")]
        public List<int> RoomsCancelled { get; set; }

        public string? CancellationReason {  get; set; }
    }
}