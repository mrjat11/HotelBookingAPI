namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class CalculateCancellationChargesResponseDTO
    {
        public decimal TotalCost { get; set; }
        public decimal CancellationCharge { get; set; }
        public decimal CancellationPercentage { get; set; }
        public string PolicyDescription { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}