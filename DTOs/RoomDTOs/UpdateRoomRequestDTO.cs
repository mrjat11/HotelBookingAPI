using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.RoomDTOs
{
    public class UpdateRoomRequestDTO
    {
        [Required]
        public int RoomID { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Room number must be up to 10 characters long.")]
        public string RoomNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Room Type ID.")]
        public int RoomTypeID { get; set; }

        [Required]
        [Range(typeof(decimal), "0.01", "999999.99", ErrorMessage = "Price must be between 0.01 and 999999.99.")]
        public decimal Price { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Bed type must be up to 50 characters long.")]
        public string BedType { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "View type must be up to 50 characters long.")]
        public string ViewType { get; set; }

        [Required]
        [RegularExpression("(Available|Under Maintenance|Occupied)", ErrorMessage = "Status must be 'Available', 'Under Maintenance', or 'Occupied'.")]
        public string Status { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }
}