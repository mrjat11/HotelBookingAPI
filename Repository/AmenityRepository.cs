using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.AmenityDTOs;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HotelBookingAPI.Repository
{
    public class AmenityRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public AmenityRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<AmenityFetchResultDTO> FetchAmenitiesAsync(bool? isActive)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spFetchAmenities", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@IsActive", (object)isActive ?? DBNull.Value);
            var statusCode = new SqlParameter("@Status", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var message = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
            command.Parameters.Add(statusCode);
            command.Parameters.Add(message);
            await connection.OpenAsync();
            var amenities = new List<AmenityDetailsDTO>();
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    amenities.Add(new AmenityDetailsDTO
                    {
                        AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
            }
            // Important: Access output parameters after closing the reader, else you will get NULL
            return new AmenityFetchResultDTO
            {
                Amenities = amenities,
                IsSuccess = Convert.ToBoolean(statusCode.Value),
                Message = message.Value.ToString()
            };
        }
        public async Task<AmenityDetailsDTO> FetchAmenityByIdAsync(int amenityId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spFetchAmenityByID", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenityId);
            await connection.OpenAsync();
            var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AmenityDetailsDTO
                {
                    AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
            else
            {
                return null;
            }
        }
        public async Task<AmenityInsertResponseDTO> AddAmenityAsync(AmenityInsertDTO amenity)
        {
            AmenityInsertResponseDTO amenityInsertResponseDTO = new AmenityInsertResponseDTO();
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spAddAmenity", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", amenity.Name);
            command.Parameters.AddWithValue("@Description", amenity.Description);
            command.Parameters.AddWithValue("@CreatedBy", "System");
            command.Parameters.Add("@AmenityID", SqlDbType.Int).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                if (Convert.ToBoolean(command.Parameters["@Status"].Value))
                {
                    amenityInsertResponseDTO.Message = Convert.ToString(command.Parameters["@Message"].Value);
                    amenityInsertResponseDTO.IsCreated = true;
                    amenityInsertResponseDTO.AmenityID = Convert.ToInt32(command.Parameters["@AmenityID"].Value);
                    return amenityInsertResponseDTO;
                }
                amenityInsertResponseDTO.Message = Convert.ToString(command.Parameters["@Message"].Value);
                amenityInsertResponseDTO.IsCreated = false;
                return amenityInsertResponseDTO;
            }
            catch (SqlException ex)
            {
                amenityInsertResponseDTO.Message = ex.Message;
                amenityInsertResponseDTO.IsCreated = false;
                return amenityInsertResponseDTO;
            }
        }
        public async Task<AmenityUpdateResponseDTO> UpdateAmenityAsync(AmenityUpdateDTO amenity)
        {
            AmenityUpdateResponseDTO amenityUpdateResponseDTO = new AmenityUpdateResponseDTO()
            {
                AmenityID = amenity.AmenityID
            };
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spUpdateAmenity", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenity.AmenityID);
            command.Parameters.AddWithValue("@Name", amenity.Name);
            command.Parameters.AddWithValue("@Description", amenity.Description);
            command.Parameters.AddWithValue("@IsActive", amenity.IsActive);
            command.Parameters.AddWithValue("@ModifiedBy", "System"); // Assume modified by system or pass a real user
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            try
            {
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                amenityUpdateResponseDTO.Message = command.Parameters["@Message"].Value.ToString();
                amenityUpdateResponseDTO.IsUpdated = Convert.ToBoolean(command.Parameters["@Status"].Value);
                return amenityUpdateResponseDTO;
            }
            catch (SqlException ex)
            {
                amenityUpdateResponseDTO.Message = ex.Message;
                amenityUpdateResponseDTO.IsUpdated = false;
                return amenityUpdateResponseDTO;
            }
        }
        public async Task<AmenityDeleteResponseDTO> DeleteAmenityAsync(int amenityId)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spDeleteAmenity", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@AmenityID", amenityId);
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityDeleteResponseDTO
            {
                IsDeleted = Convert.ToBoolean(command.Parameters["@Status"].Value),
                Message = command.Parameters["@Message"].Value.ToString()
            };
        }
        public async Task<AmenityBulkOperationResultDTO> BulkInsertAmenitiesAsync(List<AmenityInsertDTO> amenities)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spBulkInsertAmenities", connection);
            command.CommandType = CommandType.StoredProcedure;
            var amenitiesTable = new DataTable();
            amenitiesTable.Columns.Add("Name", typeof(string));
            amenitiesTable.Columns.Add("Description", typeof(string));
            amenitiesTable.Columns.Add("CreatedBy", typeof(string));
            foreach (var amenity in amenities)
            {
                amenitiesTable.Rows.Add(amenity.Name, amenity.Description, "System");
            }
            var param = command.Parameters.AddWithValue("@Amenities", amenitiesTable);
            param.SqlDbType = SqlDbType.Structured;
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(command.Parameters["@Status"].Value),
                Message = command.Parameters["@Message"].Value.ToString()
            };
        }
        public async Task<AmenityBulkOperationResultDTO> BulkUpdateAmenitiesAsync(List<AmenityUpdateDTO> amenities)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spBulkUpdateAmenities", connection);
            command.CommandType = CommandType.StoredProcedure;
            var amenitiesTable = new DataTable();
            amenitiesTable.Columns.Add("AmenityID", typeof(int));
            amenitiesTable.Columns.Add("Name", typeof(string));
            amenitiesTable.Columns.Add("Description", typeof(string));
            amenitiesTable.Columns.Add("IsActive", typeof(bool));
            foreach (var amenity in amenities)
            {
                amenitiesTable.Rows.Add(amenity.AmenityID, amenity.Name, amenity.Description, amenity.IsActive);
            }
            var param = command.Parameters.AddWithValue("@AmenityUpdates", amenitiesTable);
            param.SqlDbType = SqlDbType.Structured;
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(command.Parameters["@Status"].Value),
                Message = command.Parameters["@Message"].Value.ToString()
            };
        }
        public async Task<AmenityBulkOperationResultDTO> BulkUpdateAmenityStatusAsync(List<AmenityStatusDTO> amenityStatuses)
        {
            using var connection = _connectionFactory.CreateConnection();
            using var command = new SqlCommand("spBulkUpdateAmenityStatus", connection);
            command.CommandType = CommandType.StoredProcedure;
            var amenityStatusTable = new DataTable();
            amenityStatusTable.Columns.Add("AmenityID", typeof(int));
            amenityStatusTable.Columns.Add("IsActive", typeof(bool));
            foreach (var status in amenityStatuses)
            {
                amenityStatusTable.Rows.Add(status.AmenityID, status.IsActive);
            }
            var param = command.Parameters.AddWithValue("@AmenityStatuses", amenityStatusTable);
            param.SqlDbType = SqlDbType.Structured;
            command.Parameters.Add("@Status", SqlDbType.Bit).Direction = ParameterDirection.Output;
            command.Parameters.Add("@Message", SqlDbType.NVarChar, 255).Direction = ParameterDirection.Output;
            await connection.OpenAsync();
            await command.ExecuteNonQueryAsync();
            return new AmenityBulkOperationResultDTO
            {
                IsSuccess = Convert.ToBoolean(command.Parameters["@Status"].Value),
                Message = command.Parameters["@Message"].Value.ToString()
            };
        }
    }
}