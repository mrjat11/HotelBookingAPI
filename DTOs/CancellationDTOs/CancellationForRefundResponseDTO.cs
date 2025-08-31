namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class CancellationForRefundResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<CancellationForRefundDTO> CancellationsToRefund { get; set; }
    }
    public class CancellationForRefundDTO
    {
        public int CancellationRequestID { get; set; }
        public int ReservationID { get; set; }
        public int UserID { get; set; }
        public string CancellationType { get; set; }
        public DateTime RequestedOn { get; set; }
        public string Status { get; set; }
        public int RefundID { get; set; }
        public string RefundStatus { get; set; }
    }
}