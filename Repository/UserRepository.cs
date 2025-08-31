using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.UserDTOs;
using HotelBookingAPI.Extensions;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HotelBookingAPI.Repository
{
    public class UserRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public UserRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<CreateUserResponseDTO> AddUserAsync(CreateUserDTO user)
        {
            CreateUserResponseDTO createUserResponseDTO = new CreateUserResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spAddUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Email", user.Email);
            // Convert password to hash here 
            command.Parameters.AddWithValue("@PasswordHash", user.Password);
            command.Parameters.AddWithValue("@CreatedBy", "System");
            var userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(userIdParam);
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var UserId = (int)userIdParam.Value;
            if (UserId != -1)
            {
                createUserResponseDTO.UserId = UserId;
                createUserResponseDTO.IsCreated = true;
                createUserResponseDTO.Message = "User Created Successfully";
                return createUserResponseDTO;
            }
            var message = errorMessageParam.Value?.ToString();
            createUserResponseDTO.IsCreated = false;
            createUserResponseDTO.Message = message ?? "An unknown error occurred while creating the user.";
            return createUserResponseDTO;
        }
        public async Task<UserRoleResponseDTO> AssignRoleToUserAsync(UserRoleDTO userRole)
        {
            UserRoleResponseDTO userRoleResponseDTO = new UserRoleResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spAssignUserRole", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userRole.UserID);
            command.Parameters.AddWithValue("@RoleID", userRole.RoleID);
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var message = errorMessageParam.Value?.ToString();
            if (!string.IsNullOrEmpty(message))
            {
                userRoleResponseDTO.Message = message;
                userRoleResponseDTO.IsAssigned = false;
            }
            else
            {
                userRoleResponseDTO.Message = "User Role Assigned";
                userRoleResponseDTO.IsAssigned = true;
            }
            return userRoleResponseDTO;
        }
        public async Task<List<UserResponseDTO>> ListAllUsersAsync(bool? isActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spListAllUsers", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IsActive", (object)isActive ?? DBNull.Value);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var users = new List<UserResponseDTO>();
            while (reader.Read())
            {
                users.Add(new UserResponseDTO
                {
                    UserID = reader.GetInt32("UserID"),
                    Email = reader.GetString("Email"),
                    IsActive = reader.GetBoolean("IsActive"),
                    RoleID = reader.GetInt32("RoleID"),
                    LastLogin = reader.GetValueByColumn<DateTime?>("LastLogin"),
                });
            }
            return users;
        }
        public async Task<UserResponseDTO> GetUserByIdAsync(int userId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spGetUserByID", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return null;
            }
            var user = new UserResponseDTO
            {
                UserID = reader.GetInt32("UserID"),
                Email = reader.GetString("Email"),
                IsActive = reader.GetBoolean("IsActive"),
                RoleID = reader.GetInt32("RoleID"),
                LastLogin = reader.GetValueByColumn<DateTime?>("LastLogin"),
            };
            return user;
        }
        public async Task<UpdateUserResponseDTO> UpdateUserAsync(UpdateUserDTO user)
        {
            UpdateUserResponseDTO updateUserResponseDTO = new UpdateUserResponseDTO()
            {
                UserId = user.UserID
            };
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spUpdateUserInformation", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", user.UserID);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@ModifiedBy", "System");
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var message = errorMessageParam.Value?.ToString();
            if (string.IsNullOrEmpty(message))
            {
                updateUserResponseDTO.Message = "User Information Updated.";
                updateUserResponseDTO.IsUpdated = true;
            }
            else
            {
                updateUserResponseDTO.Message = message;
                updateUserResponseDTO.IsUpdated = false;
            }
            return updateUserResponseDTO;
        }
        public async Task<DeleteUserResponseDTO> DeleteUserAsync(int userId)
        {
            DeleteUserResponseDTO deleteUserResponseDTO = new DeleteUserResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spToggleUserActive", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@IsActive", false);
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var message = errorMessageParam.Value?.ToString();
            if (!string.IsNullOrEmpty(message))
            {
                deleteUserResponseDTO.Message = message;
                deleteUserResponseDTO.IsDeleted = false;
            }
            else
            {
                deleteUserResponseDTO.Message = "User Deleted.";
                deleteUserResponseDTO.IsDeleted = true;
            }
            return deleteUserResponseDTO;
        }
        public async Task<LoginUserResponseDTO> LoginUserAsync(LoginUserDTO login)
        {
            LoginUserResponseDTO userLoginResponseDTO = new LoginUserResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spLoginUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Email", login.Email);
            command.Parameters.AddWithValue("@PasswordHash", login.Password); // Ensure password is hashed
            var userIdParam = new SqlParameter("@UserID", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(userIdParam);
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var success = userIdParam.Value != DBNull.Value && (int)userIdParam.Value > 0;
            if (success)
            {
                var userId = Convert.ToInt32(userIdParam.Value);
                userLoginResponseDTO.UserId = userId;
                userLoginResponseDTO.IsLogin = true;
                userLoginResponseDTO.Message = "Login Successful";
                return userLoginResponseDTO;
            }
            var message = errorMessageParam.Value?.ToString();
            userLoginResponseDTO.IsLogin = false;
            userLoginResponseDTO.Message = message;
            return userLoginResponseDTO;
        }
        public async Task<(bool Success, string Message)> ToggleUserActiveAsync(int userId, bool isActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spToggleUserActive", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserID", userId);
            command.Parameters.AddWithValue("@IsActive", isActive);
            var errorMessageParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
            command.Parameters.Add(errorMessageParam);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var message = errorMessageParam.Value?.ToString();
            var success = string.IsNullOrEmpty(message);
            return (success, message);
        }
    }
}