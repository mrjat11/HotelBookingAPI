namespace HotelBookingAPI.DTOs.HotelSearchDTOs
{
    public class RoomSearchDTO
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public decimal Price { get; set; }
        public string BedType { get; set; }
        public string ViewType { get; set; }
        public string Status { get; set; }
        public RoomTypeSearchDTO RoomType { get; set; }
    }
}