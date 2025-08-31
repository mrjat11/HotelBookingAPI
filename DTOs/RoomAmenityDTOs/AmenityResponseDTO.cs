namespace HotelBookingAPI.DTOs.RoomAmenityDTOs
{
    public class AmenityResponseDTO
    {
        public int AmenityID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}