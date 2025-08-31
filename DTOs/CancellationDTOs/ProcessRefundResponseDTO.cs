namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class ProcessRefundResponseDTO
    {
        public int RefundID { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}