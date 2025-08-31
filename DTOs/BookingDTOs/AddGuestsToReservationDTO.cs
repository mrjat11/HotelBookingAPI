using System.ComponentModel.DataAnnotations;

namespace HotelBookingAPI.DTOs.BookingDTOs
{
    public class AddGuestsToReservationDTO
    {
        [Required]
        public int UserID { get; set; }
        [Required]
        public int ReservationID { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "At least one guest detail must be provided.")]
        public List<GuestDetail> GuestDetails { get; set; }  // Guest details including room association
    }

    public class GuestDetail
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string AgeGroup { get; set; }
        public string Address { get; set; }

        [Required]
        public int CountryId { get; set; }

        [Required]
        public int StateId { get; set; }

        [Required]
        public int RoomID { get; set; }  // RoomID associated with each guest
    }
}