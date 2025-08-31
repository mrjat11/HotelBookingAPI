namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class CreateCancellationResponseDTO
    {
        public int CancellationId { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}