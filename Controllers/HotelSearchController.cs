using HotelBookingAPI.DTOs.HotelSearchDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HotelSearchController : ControllerBase
    {
        private readonly HotelSearchRepository _hotelSearchRepository;
        private readonly ILogger<HotelSearchController> _logger;
        public HotelSearchController(HotelSearchRepository hotelSearchRepository, ILogger<HotelSearchController> logger)
        {
            _hotelSearchRepository = hotelSearchRepository;
            _logger = logger;
        }
        [HttpGet("Availability")]
        //checkInDate=2024-05-15
        //checkOutDate=2024-05-18
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByAvailability([FromQuery] AvailabilityHotelSearchRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Invalid Data in the Request Body");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var rooms = await _hotelSearchRepository.SearchByAvailabilityAsync(request.CheckInDate, request.CheckOutDate);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch Available Room Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get available rooms");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "Failed to get available rooms", ex.Message);
            }
        }
        [HttpGet("PriceRange")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByPriceRange([FromQuery] PriceRangeHotelSearchRequestDTO request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Invalid Price Range in the Request Body");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var rooms = await _hotelSearchRepository.SearchByPriceRangeAsync(request.MinPrice, request.MaxPrice);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by price range Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by price range");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by price range.", ex.Message);
            }
        }
        [HttpGet("RoomType")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByRoomType(string roomTypeName)
        {
            try
            {
                if (string.IsNullOrEmpty(roomTypeName))
                {
                    _logger.LogInformation("Room Type Name is Empty");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "Room Type Name is Empty");
                }
                var rooms = await _hotelSearchRepository.SearchByRoomTypeAsync(roomTypeName);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by room type Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by room type");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by room type.", ex.Message);
            }
        }
        [HttpGet("ViewType")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByViewType(string viewType)
        {
            try
            {
                if (string.IsNullOrEmpty(viewType))
                {
                    _logger.LogInformation("View Type is Empty");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "View Type is Empty");
                }
                var rooms = await _hotelSearchRepository.SearchByViewTypeAsync(viewType);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by view type Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by view type");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by view type.", ex.Message);
            }
        }
        [HttpGet("Amenities")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByAmenities(string amenityName)
        {
            try
            {
                if (string.IsNullOrEmpty(amenityName))
                {
                    _logger.LogInformation("Amenity Name is Empty");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "Amenity Name is Empty");
                }
                var rooms = await _hotelSearchRepository.SearchByAmenitiesAsync(amenityName);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by amenities Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by amenities");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by amenities.", ex.Message);
            }
        }
        [HttpGet("RoomsByType")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchRoomsByRoomTypeID(int roomTypeID)
        {
            try
            {
                if (roomTypeID <= 0)
                {
                    _logger.LogInformation($"Invalid Room Type ID, {roomTypeID}");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, $"Invalid Room Type ID, {roomTypeID}");
                }
                var rooms = await _hotelSearchRepository.SearchRoomsByRoomTypeIDAsync(roomTypeID);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by room type ID Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by room type ID");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by room type ID.", ex.Message);
            }
        }
        [HttpGet("RoomDetails")]
        public async Task<APIResponse<RoomDetailsWithAmenitiesSearchDTO>> GetRoomDetailsWithAmenitiesByRoomID(int roomID)
        {
            try
            {
                if (roomID <= 0)
                {
                    _logger.LogInformation($"Invalid Room ID, {roomID}");
                    return new APIResponse<RoomDetailsWithAmenitiesSearchDTO>(HttpStatusCode.BadRequest, $"Invalid Room ID, {roomID}");
                }
                var roomDetails = await _hotelSearchRepository.GetRoomDetailsWithAmenitiesByRoomIDAsync(roomID);
                if (roomDetails != null)
                    return new APIResponse<RoomDetailsWithAmenitiesSearchDTO>(roomDetails, "Fetch room details with amenities for RoomID Successful");
                else
                    return new APIResponse<RoomDetailsWithAmenitiesSearchDTO>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get room details with amenities for RoomID {roomID}");
                return new APIResponse<RoomDetailsWithAmenitiesSearchDTO>(HttpStatusCode.InternalServerError, "An error occurred while fetching room details with amenities.", ex.Message);
            }
        }
        [HttpGet("RoomAmenities")]
        public async Task<APIResponse<List<AmenitySearchDTO>>> GetRoomAmenitiesByRoomID(int roomID)
        {
            try
            {
                if (roomID <= 0)
                {
                    _logger.LogInformation($"Invalid Room ID, {roomID}");
                    return new APIResponse<List<AmenitySearchDTO>>(HttpStatusCode.BadRequest, $"Invalid Room ID, {roomID}");
                }
                var amenities = await _hotelSearchRepository.GetRoomAmenitiesByRoomIDAsync(roomID);
                if (amenities != null && amenities.Count > 0)
                {
                    return new APIResponse<List<AmenitySearchDTO>>(amenities, "Fetch Amenities for RoomID Successful");
                }
                return new APIResponse<List<AmenitySearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get amenities for RoomID {roomID}");
                return new APIResponse<List<AmenitySearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching room amenities.", ex.Message);
            }
        }
        [HttpGet("ByRating")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchByMinRating(float minRating)
        {
            try
            {
                if (minRating <= 0 && minRating > 5)
                {
                    _logger.LogInformation($"Invalid Rating: {minRating}");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, $"Invalid Rating: {minRating}");
                }
                var rooms = await _hotelSearchRepository.SearchByMinRatingAsync(minRating);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch rooms by minimum rating Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get rooms by minimum rating");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred while fetching rooms by minimum rating.", ex.Message);
            }
        }
        [HttpGet("CustomSearch")]
        public async Task<APIResponse<List<RoomSearchDTO>>> SearchCustomCombination([FromQuery] CustomHotelSearchCriteriaDTO criteria)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogInformation("Invalid Data in the Request Body");
                    return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var rooms = await _hotelSearchRepository.SearchCustomCombinationAsync(criteria);
                if (rooms != null && rooms.Count > 0)
                {
                    return new APIResponse<List<RoomSearchDTO>>(rooms, "Fetch Room By Custom Search Successful");
                }
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform custom search");
                return new APIResponse<List<RoomSearchDTO>>(HttpStatusCode.InternalServerError, "An error occurred during the custom search.", ex.Message);
            }
        }
    }
}