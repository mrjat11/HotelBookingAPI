namespace HotelBookingAPI.DTOs.RoomDTOs
{
    public class UpdateRoomResponseDTO
    {
        public int RoomId { get; set; }
        public string Message { get; set; }
        public bool IsUpdated { get; set; }
    }
}