using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.RoomTypeDTOs;
using HotelBookingAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HotelBookingAPI.Repository
{
    public class RoomTypeRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public RoomTypeRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<List<RoomTypeDTO>> RetrieveAllRoomTypesAsync(bool? IsActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            var command = new SqlCommand("spGetAllRoomTypes", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IsActive", (object)IsActive ?? DBNull.Value);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var roomTypes = new List<RoomTypeDTO>();
            while (reader.Read())
            {
                roomTypes.Add(new RoomTypeDTO
                {
                    RoomTypeID = reader.GetInt32("RoomTypeID"),
                    TypeName = reader.GetString("TypeName"),
                    AccessibilityFeatures = reader.GetString("AccessibilityFeatures"),
                    Description = reader.GetString("Description"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return roomTypes;
        }
        public async Task<RoomTypeDTO> RetrieveRoomTypeByIdAsync(int RoomTypeID)
        {
            using var connection = _connectionFactory.CreateConnection();
            var command = new SqlCommand("spGetRoomTypeById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomTypeID", RoomTypeID);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
            {
                return null;
            }
            var roomType = new RoomTypeDTO
            {
                RoomTypeID = RoomTypeID,
                TypeName = reader.GetString("TypeName"),
                AccessibilityFeatures = reader.GetString("AccessibilityFeatures"),
                Description = reader.GetString("Description"),
                IsActive = reader.GetBoolean("IsActive")
            };
            return roomType;
        }
        public async Task<CreateRoomTypeResponseDTO> CreateRoomType(CreateRoomTypeDTO request)
        {
            CreateRoomTypeResponseDTO createRoomTypeResponseDTO = new CreateRoomTypeResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            var command = new SqlCommand("spCreateRoomType", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@TypeName", request.TypeName));
            command.Parameters.Add(new SqlParameter("@AccessibilityFeatures", request.AccessibilityFeatures));
            command.Parameters.Add(new SqlParameter("@Description", request.Description));
            command.Parameters.Add(new SqlParameter("@CreatedBy", "System"));
            var outputId = new SqlParameter("@NewRoomTypeID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var statusCode = new SqlParameter("@StatusCode", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var message = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
            command.Parameters.Add(outputId);
            command.Parameters.Add(statusCode);
            command.Parameters.Add(message);
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                if ((int)statusCode.Value == 0)
                {
                    createRoomTypeResponseDTO.Message = message.Value.ToString();
                    createRoomTypeResponseDTO.IsCreated = true;
                    createRoomTypeResponseDTO.RoomTypeId = (int)outputId.Value;
                    return createRoomTypeResponseDTO;
                }
                createRoomTypeResponseDTO.Message = message.Value.ToString();
                createRoomTypeResponseDTO.IsCreated = false;
                return createRoomTypeResponseDTO;
            }
            catch (SqlException ex)
            {
                createRoomTypeResponseDTO.Message = ex.Message;
                createRoomTypeResponseDTO.IsCreated = false;
                return createRoomTypeResponseDTO;
            }
        }
        public async Task<UpdateRoomTypeResponseDTO> UpdateRoomType(UpdateRoomTypeDTO request)
        {
            UpdateRoomTypeResponseDTO updateRoomTypeResponseDTO = new UpdateRoomTypeResponseDTO()
            {
                RoomTypeId = request.RoomTypeID
            };
            using var connection = _connectionFactory.CreateConnection();
            var command = new SqlCommand("spUpdateRoomType", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@RoomTypeID", request.RoomTypeID));
            command.Parameters.Add(new SqlParameter("@TypeName", request.TypeName));
            command.Parameters.Add(new SqlParameter("@AccessibilityFeatures", request.AccessibilityFeatures));
            command.Parameters.Add(new SqlParameter("@Description", request.Description));
            command.Parameters.Add(new SqlParameter("@ModifiedBy", "System"));
            var statusCode = new SqlParameter("@StatusCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var message = new SqlParameter("@Message", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusCode);
            command.Parameters.Add(message);
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                updateRoomTypeResponseDTO.Message = message.Value.ToString();
                updateRoomTypeResponseDTO.IsUpdated = (int)statusCode.Value == 0;
                return updateRoomTypeResponseDTO;
            }
            catch (SqlException ex)
            {
                updateRoomTypeResponseDTO.Message = ex.Message;
                updateRoomTypeResponseDTO.IsUpdated = false;
                return updateRoomTypeResponseDTO;
            }
        }
        public async Task<DeleteRoomTypeResponseDTO> DeleteRoomType(int RoomTypeID)
        {
            DeleteRoomTypeResponseDTO deleteRoomTypeResponseDTO = new DeleteRoomTypeResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            //If you want to Delete the Record Permanentlt, then use spDeleteRoomType Stored Procedure
            var command = new SqlCommand("spToggleRoomTypeActive", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@RoomTypeID", RoomTypeID));
            command.Parameters.AddWithValue("@IsActive", false);
            var statusCode = new SqlParameter("@StatusCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var message = new SqlParameter("@Message", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusCode);
            command.Parameters.Add(message);
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                deleteRoomTypeResponseDTO.Message = "Room Type Deleted Successfully";
                deleteRoomTypeResponseDTO.IsDeleted = (int)statusCode.Value == 0;
                return deleteRoomTypeResponseDTO;
            }
            catch (SqlException ex)
            {
                deleteRoomTypeResponseDTO.Message = ex.Message;
                deleteRoomTypeResponseDTO.IsDeleted = false;
                return deleteRoomTypeResponseDTO;
            }
        }
        public async Task<(bool Success, string Message)> ToggleRoomTypeActiveAsync(int RoomTypeID, bool IsActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spToggleRoomTypeActive", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.Add(new SqlParameter("@RoomTypeID", RoomTypeID));
            command.Parameters.AddWithValue("@IsActive", IsActive);
            var statusCode = new SqlParameter("@StatusCode", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var message = new SqlParameter("@Message", SqlDbType.NVarChar, 255)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(statusCode);
            command.Parameters.Add(message);
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            var ResponseMessage = message.Value.ToString();
            var success = (int)statusCode.Value == 0;
            return (success, ResponseMessage);
        }
    }
}