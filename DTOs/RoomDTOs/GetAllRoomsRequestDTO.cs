using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.RoomDTOs
{
    public class GetAllRoomsRequestDTO
    {
        // Optional filtering by RoomTypeID; validation ensures positive integers if provided
        [Range(1, int.MaxValue, ErrorMessage = "Room Type ID must be a positive integer.")]
        public int? RoomTypeID { get; set; }

        // Optional filtering by Status; uses regex to ensure the status is one of the predefined values
        [RegularExpression("(Available|Under Maintenance|Occupied|All)", ErrorMessage = "Invalid status. Valid statuses are 'Available', 'Under Maintenance', 'Occupied', or 'All' for no filter.")]
        public string? Status { get; set; }
    }
}