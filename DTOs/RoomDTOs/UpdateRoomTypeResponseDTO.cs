namespace HotelBookingAPI.DTOs.RoomTypeDTOs
{
    public class UpdateRoomTypeResponseDTO
    {
        public int RoomTypeId { get; set; }
        public string Message { get; set; }
        public bool IsUpdated { get; set; }
    }
}