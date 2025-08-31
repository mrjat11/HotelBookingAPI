using HotelBookingAPI.DTOs.UserDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly ILogger<UserController> _logger;
        public UserController(UserRepository userRepository, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        [HttpPost("AddUser")]
        public async Task<APIResponse<CreateUserResponseDTO>> AddUser(CreateUserDTO createUserDTO)
        {
            _logger.LogInformation("Request Received for AddUser: {@CreateUserDTO}", createUserDTO);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Requrest Body");
                return new APIResponse<CreateUserResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
            }
            try
            {
                var response = await _userRepository.AddUserAsync(createUserDTO);
                _logger.LogInformation("AddUser Response From Repository: {@CreateUserResponseDTO}", response);
                if (response.IsCreated)
                {
                    return new APIResponse<CreateUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<CreateUserResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new user with email {Email}", createUserDTO.Email);
                return new APIResponse<CreateUserResponseDTO>(HttpStatusCode.InternalServerError, "Registration Failed.", ex.Message);
            }
        }
        [HttpPost("AssignRole")]
        public async Task<APIResponse<UserRoleResponseDTO>> AssignRole(UserRoleDTO userRoleDTO)
        {
            _logger.LogInformation("Request Received for AssignRole: {@UserRoleDTO}", userRoleDTO);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body");
                return new APIResponse<UserRoleResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
            }
            try
            {
                var response = await _userRepository.AssignRoleToUserAsync(userRoleDTO);
                _logger.LogInformation("AssignRole Response From Repository: {@UserRoleResponseDTO}", response);
                if (response.IsAssigned)
                {
                    return new APIResponse<UserRoleResponseDTO>(response, response.Message);
                }
                return new APIResponse<UserRoleResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleID} to user {UserID}", userRoleDTO.RoleID, userRoleDTO.UserID);
                return new APIResponse<UserRoleResponseDTO>(HttpStatusCode.InternalServerError, "Role Assigned Failed.", ex.Message);
            }
        }
        [HttpGet("AllUsers")]
        public async Task<APIResponse<List<UserResponseDTO>>> GetAllUsers(bool? isActive = null)
        {
            _logger.LogInformation($"Request Received for GetAllUsers, IsActive: {isActive}");
            try
            {
                var users = await _userRepository.ListAllUsersAsync(isActive);
                return new APIResponse<List<UserResponseDTO>>(users, "Retrieved all Users Successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing users");
                return new APIResponse<List<UserResponseDTO>>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpGet("GetUser/{userId}")]
        public async Task<APIResponse<UserResponseDTO>> GetUserById(int userId)
        {
            _logger.LogInformation($"Request Received for GetUserById, ID: {userId}");
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new APIResponse<UserResponseDTO>(HttpStatusCode.NotFound, "User not found.");
                }
                return new APIResponse<UserResponseDTO>(user, "User fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserID}", userId);
                return new APIResponse<UserResponseDTO>(HttpStatusCode.InternalServerError, "Error fetching user.", ex.Message);
            }
        }
        [HttpPut("Update/{id}")]
        public async Task<APIResponse<UpdateUserResponseDTO>> UpdateUser(int id, [FromBody] UpdateUserDTO updateUserDTO)
        {
            _logger.LogInformation("Request Received for UpdateUser {@UpdateUserDTO}", updateUserDTO);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("UpdateUser Invalid Request Body");
                return new APIResponse<UpdateUserResponseDTO>(HttpStatusCode.BadRequest, "Invalid Request Body");
            }
            if (id != updateUserDTO.UserID)
            {
                _logger.LogInformation("UpdateUser Mismatched User ID.");
                return new APIResponse<UpdateUserResponseDTO>(HttpStatusCode.BadRequest, "Mismatched User ID.");
            }
            try
            {
                var response = await _userRepository.UpdateUserAsync(updateUserDTO);
                if (response.IsUpdated)
                {
                    return new APIResponse<UpdateUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<UpdateUserResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserID}", updateUserDTO.UserID);
                return new APIResponse<UpdateUserResponseDTO>(HttpStatusCode.InternalServerError, "Update Failed.", ex.Message);
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<APIResponse<DeleteUserResponseDTO>> DeleteUser(int id)
        {
            _logger.LogInformation($"Request Received for DeleteUser, Id: {id}");
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return new APIResponse<DeleteUserResponseDTO>(HttpStatusCode.NotFound, "User not found.");
                }
                var response = await _userRepository.DeleteUserAsync(id);
                if (response.IsDeleted)
                {
                    return new APIResponse<DeleteUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<DeleteUserResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserID}", id);
                return new APIResponse<DeleteUserResponseDTO>(HttpStatusCode.InternalServerError, "Internal server error: " + ex.Message);
            }
        }
        [HttpPost("Login")]
        public async Task<APIResponse<LoginUserResponseDTO>> LoginUser([FromBody] LoginUserDTO loginUserDTO)
        {
            _logger.LogInformation("Request Received for LoginUser {@LoginUserDTO}", loginUserDTO);
            if (!ModelState.IsValid)
            {
                return new APIResponse<LoginUserResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Requrest Body");
            }
            try
            {
                var response = await _userRepository.LoginUserAsync(loginUserDTO);
                if (response.IsLogin)
                {
                    return new APIResponse<LoginUserResponseDTO>(response, response.Message);
                }
                return new APIResponse<LoginUserResponseDTO>(HttpStatusCode.BadRequest, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user with email {Email}", loginUserDTO.Email);
                return new APIResponse<LoginUserResponseDTO>(HttpStatusCode.InternalServerError, "Login failed.", ex.Message);
            }
        }
        [HttpPost("ToggleActive")]
        public async Task<IActionResult> ToggleActive(int userId, bool isActive)
        {
            try
            {
                var result = await _userRepository.ToggleUserActiveAsync(userId, isActive);
                if (result.Success)
                    return Ok(new { Message = "User activation status updated successfully." });
                else
                    return BadRequest(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for user {UserID}", userId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}