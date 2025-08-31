using HotelBookingAPI.DTOs.AmenityDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AmenityController : ControllerBase
    {
        private readonly AmenityRepository _amenityRepository;
        private readonly ILogger<AmenityController> _logger;
        public AmenityController(AmenityRepository amenityRepository, ILogger<AmenityController> logger)
        {
            _amenityRepository = amenityRepository;
            _logger = logger;
        }
        [HttpGet("Fetch")]
        public async Task<APIResponse<AmenityFetchResultDTO>> FetchAmenities(bool? isActive = null)
        {
            try
            {
                var response = await _amenityRepository.FetchAmenitiesAsync(isActive);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityFetchResultDTO>(response, "Retrieved all Room Amenity Successfully.");
                }
                return new APIResponse<AmenityFetchResultDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching amenities.");
                return new APIResponse<AmenityFetchResultDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpGet("Fetch/{id}")]
        public async Task<APIResponse<AmenityDetailsDTO>> FetchAmenityById(int id)
        {
            try
            {
                var response = await _amenityRepository.FetchAmenityByIdAsync(id);
                if (response != null)
                {
                    return new APIResponse<AmenityDetailsDTO>(response, "Retrieved Room Amenity Successfully.");
                }
                return new APIResponse<AmenityDetailsDTO>(HttpStatusCode.NotFound, "Amenity ID not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching amenity by ID.");
                return new APIResponse<AmenityDetailsDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpPost("Add")]
        public async Task<APIResponse<AmenityInsertResponseDTO>> AddAmenity([FromBody] AmenityInsertDTO amenity)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityInsertResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
                }
                var response = await _amenityRepository.AddAmenityAsync(amenity);
                if (response.IsCreated)
                {
                    return new APIResponse<AmenityInsertResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityInsertResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding amenity.");
                return new APIResponse<AmenityInsertResponseDTO>(HttpStatusCode.InternalServerError, "Amenity Creation Failed.", ex.Message);
            }
        }
        [HttpPut("Update/{id}")]
        public async Task<APIResponse<AmenityUpdateResponseDTO>> UpdateAmenity(int id, [FromBody] AmenityUpdateDTO amenity)
        {
            try
            {
                if (id != amenity.AmenityID)
                {
                    _logger.LogInformation("UpdateRoom Mismatched Amenity ID");
                    return new APIResponse<AmenityUpdateResponseDTO>(HttpStatusCode.BadRequest, "Mismatched Amenity ID.");
                }
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityUpdateResponseDTO>(HttpStatusCode.BadRequest, "Invalid Request Body");
                }
                var response = await _amenityRepository.UpdateAmenityAsync(amenity);
                if (response.IsUpdated)
                {
                    return new APIResponse<AmenityUpdateResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityUpdateResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating amenity.");
                return new APIResponse<AmenityUpdateResponseDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<APIResponse<AmenityDeleteResponseDTO>> DeleteAmenity(int id)
        {
            try
            {
                var amenity = await _amenityRepository.FetchAmenityByIdAsync(id);
                if (amenity == null)
                {
                    return new APIResponse<AmenityDeleteResponseDTO>(HttpStatusCode.NotFound, "Amenity not found.");
                }
                var response = await _amenityRepository.DeleteAmenityAsync(id);
                if (response.IsDeleted)
                {
                    return new APIResponse<AmenityDeleteResponseDTO>(response, response.Message);
                }
                return new APIResponse<AmenityDeleteResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting amenity.");
                return new APIResponse<AmenityDeleteResponseDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpPost("BulkInsert")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkInsertAmenities(List<AmenityInsertDTO> amenities)
        {
            try
            {
                var response = await _amenityRepository.BulkInsertAmenitiesAsync(amenities);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk inserting amenities.");
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpPost("BulkUpdate")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkUpdateAmenities(List<AmenityUpdateDTO> amenities)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var response = await _amenityRepository.BulkUpdateAmenitiesAsync(amenities);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating amenities.");
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
        [HttpPut("BulkUpdateStatus")]
        public async Task<APIResponse<AmenityBulkOperationResultDTO>> BulkUpdateAmenityStatus(List<AmenityStatusDTO> amenityStatuses)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
                }
                var response = await _amenityRepository.BulkUpdateAmenityStatusAsync(amenityStatuses);
                if (response.IsSuccess)
                {
                    return new APIResponse<AmenityBulkOperationResultDTO>(response, response.Message);
                }
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating amenity status.");
                return new APIResponse<AmenityBulkOperationResultDTO>(HttpStatusCode.InternalServerError, "An error occurred while processing your request.", ex.Message);
            }
        }
    }
}