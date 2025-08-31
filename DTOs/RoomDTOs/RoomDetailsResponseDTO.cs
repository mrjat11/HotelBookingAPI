namespace HotelBookingAPI.DTOs.RoomDTOs
{
    public class RoomDetailsResponseDTO
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public int RoomTypeID { get; set; }
        public decimal Price { get; set; }
        public string BedType { get; set; }
        public string ViewType { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }
}