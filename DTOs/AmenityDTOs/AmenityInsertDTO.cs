using System.ComponentModel.DataAnnotations;
namespace HotelBookingAPI.DTOs.AmenityDTOs
{
    public class AmenityInsertDTO
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name length can't be more than 100 characters.")]
        public string Name { get; set; }
        [StringLength(255, ErrorMessage = "Description length can't be more than 255 characters.")]
        public string Description { get; set; }
    }
}