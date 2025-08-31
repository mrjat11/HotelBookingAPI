namespace HotelBookingAPI.DTOs.CancellationDTOs
{
    public class CancellationPoliciesResponseDTO
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public List<CancellationPolicyDTO> Policies { get; set; }
    }

    public class CancellationPolicyDTO
    {
        public int PolicyID { get; set; }
        public string Description { get; set; }
        public decimal CancellationChargePercentage { get; set; }
        public decimal MinimumCharge { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime EffectiveToDate { get; set; }
    }
}