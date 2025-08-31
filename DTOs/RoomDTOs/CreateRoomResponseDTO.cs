namespace HotelBookingAPI.DTOs.RoomDTOs
{
    public class CreateRoomResponseDTO
    {
        public int RoomID { get; set; }
        public string Message { get; set; }
        public bool IsCreated { get; set; }
    }
}