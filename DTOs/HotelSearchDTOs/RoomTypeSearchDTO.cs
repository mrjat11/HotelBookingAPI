using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBookingAPI.DTOs.HotelSearchDTOs
{
    public class RoomTypeSearchDTO
    {
        public int RoomTypeID { get; set; }
        public string TypeName { get; set; }
        public string AccessibilityFeatures { get; set; }
        public string Description { get; set; }
    }
}