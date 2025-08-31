namespace HotelBookingAPI.DTOs.RoomTypeDTOs
{
    public class CreateRoomTypeResponseDTO
    {
        public int RoomTypeId { get; set; }
        public string Message { get; set; }
        public bool IsCreated { get; set; }
    }
}