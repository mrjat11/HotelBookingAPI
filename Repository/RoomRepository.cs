using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.RoomDTOs;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.PortableExecutable;
namespace HotelBookingAPI.Repository
{
    public class RoomRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public RoomRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<CreateRoomResponseDTO> CreateRoomAsync(CreateRoomRequestDTO request)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spCreateRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomNumber", request.RoomNumber);
            command.Parameters.AddWithValue("@RoomTypeID", request.RoomTypeID);
            command.Parameters.AddWithValue("@Price", request.Price);
            command.Parameters.AddWithValue("@BedType", request.BedType);
            command.Parameters.AddWithValue("@ViewType", request.ViewType);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
            command.Parameters.AddWithValue("@CreatedBy", "System");
            command.Parameters.Add("@NewRoomID", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                var outputRoomID = command.Parameters["@NewRoomID"].Value;
                var newRoomID = outputRoomID != DBNull.Value ? Convert.ToInt32(outputRoomID) : 0;  // Safely handle potential DBNull values.
                return new CreateRoomResponseDTO
                {
                    RoomID = newRoomID,
                    IsCreated = (int)command.Parameters["@StatusCode"].Value == 0,
                    Message = (string)command.Parameters["@Message"].Value
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating room: {ex.Message}", ex);
            }
        }
        public async Task<UpdateRoomResponseDTO> UpdateRoomAsync(UpdateRoomRequestDTO request)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spUpdateRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomID", request.RoomID);
            command.Parameters.AddWithValue("@RoomNumber", request.RoomNumber);
            command.Parameters.AddWithValue("@RoomTypeID", request.RoomTypeID);
            command.Parameters.AddWithValue("@Price", request.Price);
            command.Parameters.AddWithValue("@BedType", request.BedType);
            command.Parameters.AddWithValue("@ViewType", request.ViewType);
            command.Parameters.AddWithValue("@Status", request.Status);
            command.Parameters.AddWithValue("@IsActive", request.IsActive);
            command.Parameters.AddWithValue("@ModifiedBy", "System");
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                return new UpdateRoomResponseDTO
                {
                    RoomId = request.RoomID,
                    IsUpdated = (int)command.Parameters["@StatusCode"].Value == 0,
                    Message = (string)command.Parameters["@Message"].Value
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating room: {ex.Message}", ex);
            }
        }
        public async Task<DeleteRoomResponseDTO> DeleteRoomAsync(int roomId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spDeleteRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomID", roomId);
            command.Parameters.Add("@StatusCode", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                return new DeleteRoomResponseDTO
                {
                    IsDeleted = (int)command.Parameters["@StatusCode"].Value == 0,
                    Message = (string)command.Parameters["@Message"].Value
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting room: {ex.Message}", ex);
            }
        }
        public async Task<RoomDetailsResponseDTO> GetRoomByIdAsync(int roomId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spGetRoomById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@RoomID", roomId);
            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RoomDetailsResponseDTO
                    {
                        RoomID = reader.GetInt32("RoomID"),
                        RoomNumber = reader.GetString("RoomNumber"),
                        RoomTypeID = reader.GetInt32("RoomTypeID"),
                        Price = reader.GetDecimal("Price"),
                        BedType = reader.GetString("BedType"),
                        ViewType = reader.GetString("ViewType"),
                        Status = reader.GetString("Status"),
                        IsActive = reader.GetBoolean("IsActive")
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving room by ID: {ex.Message}", ex);
            }
        }
        public async Task<List<RoomDetailsResponseDTO>> GetAllRoomsAsync(GetAllRoomsRequestDTO request)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spGetAllRoom", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            // Add parameters for RoomTypeID and Status, handling nulls appropriately
            command.Parameters.Add(new SqlParameter("@RoomTypeID", SqlDbType.Int)
            {
                Value = request.RoomTypeID.HasValue ? (object)request.RoomTypeID.Value : DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50)
            {
                Value = string.IsNullOrEmpty(request.Status) ? DBNull.Value : (object)request.Status
            });
            try
            {
                await connection.OpenAsync();
                var rooms = new List<RoomDetailsResponseDTO>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    rooms.Add(new RoomDetailsResponseDTO
                    {
                        RoomID = reader.GetInt32("RoomID"),
                        RoomNumber = reader.GetString("RoomNumber"),
                        RoomTypeID = reader.GetInt32("RoomTypeID"),
                        Price = reader.GetDecimal("Price"),
                        BedType = reader.GetString("BedType"),
                        ViewType = reader.GetString("ViewType"),
                        Status = reader.GetString("Status"),
                        IsActive = reader.GetBoolean("IsActive")
                    });
                }
                return rooms;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving all rooms: {ex.Message}", ex);
            }
        }
    }
}