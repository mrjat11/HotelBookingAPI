namespace HotelBookingAPI.DTOs.PaymentDTOs
{
    public class ProcessPaymentResponseDTO
    {
        public int PaymentID { get; set; }      
        public bool Status { get; set; }        
        public string Message { get; set; }     
    }
}