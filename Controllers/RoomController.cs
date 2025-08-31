using HotelBookingAPI.DTOs.RoomDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RoomRepository _roomRepository;
        private readonly ILogger<RoomController> _logger;
        public RoomController(RoomRepository roomRepository, ILogger<RoomController> logger)
        {
            _roomRepository = roomRepository;
            _logger = logger;
        }
        [HttpGet("All")]
        public async Task<APIResponse<List<RoomDetailsResponseDTO>>> GetAllRooms([FromQuery] GetAllRoomsRequestDTO request)
        {
            _logger.LogInformation("Request Received for CreateRoomType: {@GetAllRoomsRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<List<RoomDetailsResponseDTO>>(HttpStatusCode.BadRequest, "Invalid Data in the Query String");
            }
            try
            {
                var rooms = await _roomRepository.GetAllRoomsAsync(request);
                return new APIResponse<List<RoomDetailsResponseDTO>>(rooms, "Retrieved all Room Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Retriving all Room");
                return new APIResponse<List<RoomDetailsResponseDTO>>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<APIResponse<RoomDetailsResponseDTO>> GetRoomById(int id)
        {
            _logger.LogInformation($"Request Received for GetRoomById, id: {id}");
            try
            {
                var response = await _roomRepository.GetRoomByIdAsync(id);
                if (response == null)
                {
                    return new APIResponse<RoomDetailsResponseDTO>(HttpStatusCode.NotFound, "Room ID not found.");
                }
                return new APIResponse<RoomDetailsResponseDTO>(response, "Room fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Room by ID {id}", id);
                return new APIResponse<RoomDetailsResponseDTO>(HttpStatusCode.BadRequest, "Error fetching Room.", ex.Message);
            }
        }
        [HttpPost("Create")]
        public async Task<APIResponse<CreateRoomResponseDTO>> CreateRoom([FromBody] CreateRoomRequestDTO request)
        {
            _logger.LogInformation("Request Received for CreateRoom: {@CreateRoomRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<CreateRoomResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
            }
            try
            {
                var response = await _roomRepository.CreateRoomAsync(request);
                _logger.LogInformation("CreateRoom Response From Repository: {@CreateRoomResponseDTO}", response);
                if (response.IsCreated)
                {
                    return new APIResponse<CreateRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<CreateRoomResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new Room");
                return new APIResponse<CreateRoomResponseDTO>(HttpStatusCode.InternalServerError, "Room Creation Failed.", ex.Message);
            }
        }
        [HttpPut("Update/{id}")]
        public async Task<APIResponse<UpdateRoomResponseDTO>> UpdateRoom(int id, [FromBody] UpdateRoomRequestDTO request)
        {
            _logger.LogInformation("Request Received for UpdateRoom {@UpdateRoomRequestDTO}", request);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("UpdateRoom Invalid Request Body");
                return new APIResponse<UpdateRoomResponseDTO>(HttpStatusCode.BadRequest, "Invalid Request Body");
            }
            if (id != request.RoomID)
            {
                _logger.LogInformation("UpdateRoom Mismatched Room ID");
                return new APIResponse<UpdateRoomResponseDTO>(HttpStatusCode.BadRequest, "Mismatched Room ID.");
            }
            try
            {
                var response = await _roomRepository.UpdateRoomAsync(request);
                if (response.IsUpdated)
                {
                    return new APIResponse<UpdateRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<UpdateRoomResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Updating Room {id}", id);
                return new APIResponse<UpdateRoomResponseDTO>(HttpStatusCode.InternalServerError, "Update Room Failed.", ex.Message);
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<APIResponse<DeleteRoomResponseDTO>> DeleteRoom(int id)
        {
            _logger.LogInformation($"Request Received for DeleteRoom, id: {id}");
            try
            {
                var room = await _roomRepository.GetRoomByIdAsync(id);
                if (room == null)
                {
                    return new APIResponse<DeleteRoomResponseDTO>(HttpStatusCode.NotFound, "Room not found.");
                }
                var response = await _roomRepository.DeleteRoomAsync(id);
                if (response.IsDeleted)
                {
                    return new APIResponse<DeleteRoomResponseDTO>(response, response.Message);
                }
                return new APIResponse<DeleteRoomResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Room {id}", id);
                return new APIResponse<DeleteRoomResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
    }
}