namespace HotelBookingAPI.DTOs.RoomAmenityDTOs
{
    public class FetchRoomAmenityResponseDTO
    {
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; }
        public string Description { get; set; }
        public string AccessibilityFeatures { get; set; }
        public bool IsActive { get; set; }
    }
}