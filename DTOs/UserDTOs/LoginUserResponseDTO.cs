namespace HotelBookingAPI.DTOs.UserDTOs
{
    public class LoginUserResponseDTO
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsLogin { get; set; }
    }
}