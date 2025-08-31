using HotelBookingAPI.DTOs.RoomAmenityDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomAmenityController : ControllerBase
    {
        private readonly RoomAmenityRepository _roomAmenityRepository;
        private readonly ILogger<RoomAmenityController> _logger;
        public RoomAmenityController(RoomAmenityRepository roomAmenityRepository, ILogger<RoomAmenityController> logger)
        {
            _roomAmenityRepository = roomAmenityRepository;
            _logger = logger;
        }
        [HttpGet("FetchAmenitiesByRoomTypeId/{roomTypeId}")]
        public async Task<APIResponse<List<AmenityResponseDTO>>> FetchAmenitiesByRoomTypeId(int roomTypeId)
        {
            try
            {
                var amenities = await _roomAmenityRepository.FetchRoomAmenitiesByRoomTypeIdAsync(roomTypeId);
                if (amenities != null && amenities.Count > 0)
                {
                    return new APIResponse<List<AmenityResponseDTO>>(amenities, "Fetch Amenities By Room Type Id Successfully.");
                }
                return new APIResponse<List<AmenityResponseDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching amenities by room type ID");
                return new APIResponse<List<AmenityResponseDTO>>(HttpStatusCode.InternalServerError, "Error fetching amenities by room type ID", ex.Message);
            }
        }
        [HttpGet("FetchRoomTypesByAmenityId/{amenityId}")]
        public async Task<APIResponse<List<FetchRoomAmenityResponseDTO>>> FetchRoomTypesByAmenityId(int amenityId)
        {
            try
            {
                var roomTypes = await _roomAmenityRepository.FetchRoomTypesByAmenityIdAsync(amenityId);
                if (roomTypes != null && roomTypes.Count > 0)
                {
                    return new APIResponse<List<FetchRoomAmenityResponseDTO>>(roomTypes, "Fetch Room Types By AmenityId Successfully.");
                }
                return new APIResponse<List<FetchRoomAmenityResponseDTO>>(HttpStatusCode.BadRequest, "No Record Found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching room types by amenity ID");
                return new APIResponse<List<FetchRoomAmenityResponseDTO>>(HttpStatusCode.InternalServerError, "Error fetching room types by amenity ID", ex.Message);
            }
        }
        [HttpPost("AddRoomAmenity")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> AddRoomAmenity([FromBody] RoomAmenityDTO input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var response = await _roomAmenityRepository.AddRoomAmenityAsync(input);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding room amenity");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error adding room amenity", ex.Message);
            }
        }
        [HttpPost("DeleteRoomAmenity")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> DeleteRoomAmenity([FromBody] RoomAmenityDTO input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var response = await _roomAmenityRepository.DeleteRoomAmenityAsync(input);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room amenity");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error deleting room amenity", ex.Message);
            }
        }
        [HttpPost("BulkInsertRoomAmenities")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> BulkInsertRoomAmenities([FromBody] RoomAmenitiesBulkInsertUpdateDTO input)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var response = await _roomAmenityRepository.BulkInsertRoomAmenitiesAsync(input);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk insert of room amenities");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error performing bulk insert of room amenities", ex.Message);
            }
        }
        [HttpPost("BulkUpdateRoomAmenities")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> BulkUpdateRoomAmenities([FromBody] RoomAmenitiesBulkInsertUpdateDTO input)
        {
            try
            {
                var response = await _roomAmenityRepository.BulkUpdateRoomAmenitiesAsync(input);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update of room amenities");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error performing bulk update of room amenities", ex.Message);
            }
        }
        [HttpPost("DeleteAllRoomAmenitiesByRoomTypeID/{roomTypeId}")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> DeleteAllRoomAmenitiesByRoomTypeID(int roomTypeId)
        {
            try
            {
                var response = await _roomAmenityRepository.DeleteAllRoomAmenitiesByRoomTypeIDAsync(roomTypeId);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all room amenities by room type ID");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error deleting all room amenities by room type ID", ex.Message);
            }
        }
        [HttpPost("DeleteAllRoomAmenitiesByAmenityID/{amenityId}")]
        public async Task<APIResponse<RoomAmenityResponseDTO>> DeleteAllRoomAmenitiesByAmenityID(int amenityId)
        {
            try
            {
                var response = await _roomAmenityRepository.DeleteAllRoomAmenitiesByAmenityIDAsync(amenityId);
                if (response.IsSuccess)
                {
                    return new APIResponse<RoomAmenityResponseDTO>(response, response.Message);
                }
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all room amenities by amenity ID");
                return new APIResponse<RoomAmenityResponseDTO>(HttpStatusCode.InternalServerError, "Error deleting all room amenities by amenity ID", ex.Message);
            }
        }
    }
}