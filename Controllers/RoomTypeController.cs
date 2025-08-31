using HotelBookingAPI.DTOs.RoomTypeDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypeController : ControllerBase
    {
        private readonly RoomTypeRepository _roomTypeRepository;
        private readonly ILogger<RoomTypeController> _logger;
        public RoomTypeController(RoomTypeRepository roomTypeRepository, ILogger<RoomTypeController> logger)
        {
            _roomTypeRepository = roomTypeRepository;
            _logger = logger;
        }
        [HttpGet("AllRoomTypes")]
        public async Task<APIResponse<List<RoomTypeDTO>>> GetAllRoomTypes(bool? IsActive = null)
        {
            _logger.LogInformation($"Request Received for GetAllRoomTypes, IsActive: {IsActive}");
            try
            {
                var users = await _roomTypeRepository.RetrieveAllRoomTypesAsync(IsActive);
                return new APIResponse<List<RoomTypeDTO>>(users, "Retrieved all Room Types Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Retriving all Room Types");
                return new APIResponse<List<RoomTypeDTO>>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("GetRoomType/{RoomTypeID}")]
        public async Task<APIResponse<RoomTypeDTO>> GetRoomTypeById(int RoomTypeID)
        {
            _logger.LogInformation($"Request Received for GetRoomTypeById, RoomTypeID: {RoomTypeID}");
            try
            {
                var roomType = await _roomTypeRepository.RetrieveRoomTypeByIdAsync(RoomTypeID);
                if (roomType == null)
                {
                    return new APIResponse<RoomTypeDTO>(HttpStatusCode.NotFound, "RoomTypeID not found.");
                }
                return new APIResponse<RoomTypeDTO>(roomType, "RoomType fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Room Type by ID {RoomTypeID}", RoomTypeID);
                return new APIResponse<RoomTypeDTO>(HttpStatusCode.BadRequest, "Error fetching Room Type .", ex.Message);
            }
        }
        [HttpPost("AddRoomType")]
        public async Task<APIResponse<CreateRoomTypeResponseDTO>> CreateRoomType([FromBody] CreateRoomTypeDTO request)
        {
            _logger.LogInformation("Request Received for CreateRoomType: {@CreateRoomTypeDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<CreateRoomTypeResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
            }
            try
            {
                var response = await _roomTypeRepository.CreateRoomType(request);
                _logger.LogInformation("CreateRoomType Response From Repository: {@CreateRoomTypeResponseDTO}", response);
                if (response.IsCreated)
                {
                    return new APIResponse<CreateRoomTypeResponseDTO>(response, response.Message);
                }
                return new APIResponse<CreateRoomTypeResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new Room Type with Name {TypeName}", request.TypeName);
                return new APIResponse<CreateRoomTypeResponseDTO>(HttpStatusCode.InternalServerError, "Room Type Creation Failed.", ex.Message);
            }
        }
        [HttpPut("Update/{RoomTypeId}")]
        public async Task<APIResponse<UpdateRoomTypeResponseDTO>> UpdateRoomType(int RoomTypeId, [FromBody] UpdateRoomTypeDTO request)
        {
            _logger.LogInformation("Request Received for UpdateRoomType {@UpdateRoomTypeDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("UpdateRoomType Invalid Request Body");
                return new APIResponse<UpdateRoomTypeResponseDTO>(HttpStatusCode.BadRequest, "Invalid Request Body");
            }
            if (RoomTypeId != request.RoomTypeID)
            {
                _logger.LogInformation("UpdateRoomType Mismatched Room Type ID");
                return new APIResponse<UpdateRoomTypeResponseDTO>(HttpStatusCode.BadRequest, "Mismatched Room Type ID.");
            }
            try
            {
                var response = await _roomTypeRepository.UpdateRoomType(request);
                if (response.IsUpdated)
                {
                    return new APIResponse<UpdateRoomTypeResponseDTO>(response, response.Message);
                }
                return new APIResponse<UpdateRoomTypeResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Room Type {RoomTypeId}", RoomTypeId);
                return new APIResponse<UpdateRoomTypeResponseDTO>(HttpStatusCode.InternalServerError, "Update Room Type Failed.", ex.Message);
            }
        }
        [HttpDelete("Delete/{RoomTypeId}")]
        public async Task<APIResponse<DeleteRoomTypeResponseDTO>> DeleteRoomType(int RoomTypeId)
        {
            _logger.LogInformation($"Request Received for DeleteRoomType, RoomTypeId: {RoomTypeId}");
            try
            {
                var roomType = await _roomTypeRepository.RetrieveRoomTypeByIdAsync(RoomTypeId);
                if (roomType == null)
                {
                    return new APIResponse<DeleteRoomTypeResponseDTO>(HttpStatusCode.NotFound, "RoomType not found.");
                }
                var response = await _roomTypeRepository.DeleteRoomType(RoomTypeId);
                if (response.IsDeleted)
                {
                    return new APIResponse<DeleteRoomTypeResponseDTO>(response, response.Message);
                }
                return new APIResponse<DeleteRoomTypeResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RoomType {RoomTypeId}", RoomTypeId);
                return new APIResponse<DeleteRoomTypeResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpPost("ActiveInActive")]
        public async Task<IActionResult> ToggleActive(int RoomTypeId, bool IsActive)
        {
            try
            {
                var result = await _roomTypeRepository.ToggleRoomTypeActiveAsync(RoomTypeId, IsActive);
                if (result.Success)
                    return Ok(new { Message = "RoomType activation status updated successfully." });
                else
                    return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for RoomTypeId {RoomTypeId}", RoomTypeId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}