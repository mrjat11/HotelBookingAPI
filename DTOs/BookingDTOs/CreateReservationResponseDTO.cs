namespace HotelBookingAPI.DTOs.BookingDTOs
{
    public class CreateReservationResponseDTO
    {
        public int ReservationID { get; set; }  
        public bool Status { get; set; }        
        public string Message { get; set; }     
    }
}