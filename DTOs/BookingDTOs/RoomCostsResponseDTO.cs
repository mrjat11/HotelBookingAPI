namespace HotelBookingAPI.DTOs.BookingDTOs
{
    public class RoomCostsResponseDTO
    {
        public List<RoomCostDetailDTO> RoomDetails { get; set; } = new List<RoomCostDetailDTO>();
        public decimal Amount { get; set; }     // Base total cost before tax
        public decimal GST { get; set; }        // GST amount based on 18%
        public decimal TotalAmount { get; set; }  // Total cost including GST
        public bool Status { get; set; }        
        public string Message { get; set; }     
    }

    public class RoomCostDetailDTO
    {
        public int RoomID { get; set; }
        public string RoomNumber { get; set; }
        public decimal RoomPrice { get; set; }       // Cost for individual room
        public int NumberOfNights { get; set; }      
        public decimal TotalPrice { get; set; }       // Cost for individual room multiplied by Number of Nights
    }
}