using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.DTOs.RoomAmenityDTOs
{
    public class RoomAmenityDTO
    {
        [Required]
        public int RoomTypeID { get; set; }
        [Required]
        public int AmenityID { get; set; }
    }
}