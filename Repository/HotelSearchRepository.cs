using System.Data;
using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.HotelSearchDTOs;
using HotelBookingAPI.Extensions;
using Microsoft.Data.SqlClient;
namespace HotelBookingAPI.Repository
{
    public class HotelSearchRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public HotelSearchRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        // Search by Availability Dates
        // spSearchByAvailability
        public async Task<List<RoomSearchDTO>> SearchByAvailabilityAsync(DateTime checkInDate, DateTime checkOutDate)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByAvailability", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CheckInDate", checkInDate));
                    command.Parameters.Add(new SqlParameter("@CheckOutDate", checkOutDate));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Search by Price Range
        // spSearchByPriceRange
        public async Task<List<RoomSearchDTO>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByPriceRange", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MinPrice", minPrice));
                    command.Parameters.Add(new SqlParameter("@MaxPrice", maxPrice));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Search by Room Type
        // spSearchByRoomType
        public async Task<List<RoomSearchDTO>> SearchByRoomTypeAsync(string roomTypeName)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByRoomType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RoomTypeName", roomTypeName));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Search by View Type
        // spSearchByViewType
        public async Task<List<RoomSearchDTO>> SearchByViewTypeAsync(string viewType)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByViewType", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@ViewType", viewType));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Search by Amenities
        // spSearchByAmenities
        public async Task<List<RoomSearchDTO>> SearchByAmenitiesAsync(string amenityName)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByAmenities", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@AmenityName", amenityName));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Search All Rooms by RoomTypeID
        // spSearchRoomsByRoomTypeID
        public async Task<List<RoomSearchDTO>> SearchRoomsByRoomTypeIDAsync(int roomTypeID)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchRoomsByRoomTypeID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RoomTypeID", roomTypeID));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            rooms.Add(CreateRoomSearchDTO(reader));
                        }
                    }
                }
            }
            return rooms;
        }
        // Fetch Room Details with Amenities by RoomID
        // spGetRoomDetailsWithAmenitiesByRoomID
        public async Task<RoomDetailsWithAmenitiesSearchDTO> GetRoomDetailsWithAmenitiesByRoomIDAsync(int roomID)
        {
            RoomDetailsWithAmenitiesSearchDTO roomDetails = new RoomDetailsWithAmenitiesSearchDTO();
            //List<AmenitySearchDTO> amenities = new List<AmenitySearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spGetRoomDetailsWithAmenitiesByRoomID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RoomID", roomID));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            roomDetails.Room = CreateRoomSearchDTO(reader);
                            roomDetails.Amenities = new List<AmenitySearchDTO>();
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    roomDetails.Amenities.Add(new AmenitySearchDTO
                                    {
                                        AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                                        Name = reader.GetString(reader.GetOrdinal("Name")),
                                        Description = reader.GetString(reader.GetOrdinal("Description"))
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return roomDetails;
        }
        // Method to retrieve all amenities for a specific room by its RoomID
        // spGetRoomAmenitiesByRoomID
        public async Task<List<AmenitySearchDTO>> GetRoomAmenitiesByRoomIDAsync(int roomID)
        {
            var amenities = new List<AmenitySearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spGetRoomAmenitiesByRoomID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@RoomID", roomID));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            amenities.Add(new AmenitySearchDTO
                            {
                                AmenityID = reader.GetInt32(reader.GetOrdinal("AmenityID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetValueByColumn<string>("Description")
                            });
                        }
                    }
                }
            }
            return amenities;
        }
        // Search by Minimum Rating
        // spSearchByMinRating
        public async Task<List<RoomSearchDTO>> SearchByMinRatingAsync(float minRating)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var command = new SqlCommand("spSearchByMinRating", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@MinRating", minRating));
                    connection.Open();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                rooms.Add(CreateRoomSearchDTO(reader));
                            }
                        }
                    }
                }
            }
            return rooms;
        }
        // Search Custom Combination
        public async Task<List<RoomSearchDTO>> SearchCustomCombinationAsync(CustomHotelSearchCriteriaDTO criteria)
        {
            var rooms = new List<RoomSearchDTO>();
            using (var connection = _connectionFactory.CreateConnection())
            {
                var command = new SqlCommand("spSearchCustomCombination", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@MinPrice", (object)criteria.MinPrice ?? DBNull.Value);
                command.Parameters.AddWithValue("@MaxPrice", (object)criteria.MaxPrice ?? DBNull.Value);
                command.Parameters.AddWithValue("@RoomTypeName", string.IsNullOrEmpty(criteria.RoomTypeName) ? DBNull.Value : criteria.RoomTypeName);
                command.Parameters.AddWithValue("@AmenityName", string.IsNullOrEmpty(criteria.AmenityName) ? DBNull.Value : criteria.AmenityName);
                command.Parameters.AddWithValue("@ViewType", string.IsNullOrEmpty(criteria.ViewType) ? DBNull.Value : criteria.ViewType);
                connection.Open();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rooms.Add(CreateRoomSearchDTO(reader));
                    }
                }
            }
            return rooms;
        }
        private RoomSearchDTO CreateRoomSearchDTO(SqlDataReader reader)
        {
            return new RoomSearchDTO
            {
                RoomID = reader.GetInt32(reader.GetOrdinal("RoomID")),
                RoomNumber = reader.GetString(reader.GetOrdinal("RoomNumber")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                BedType = reader.GetString(reader.GetOrdinal("BedType")),
                ViewType = reader.GetString(reader.GetOrdinal("ViewType")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                RoomType = new RoomTypeSearchDTO
                {
                    RoomTypeID = reader.GetInt32(reader.GetOrdinal("RoomTypeID")),
                    TypeName = reader.GetString(reader.GetOrdinal("TypeName")),
                    AccessibilityFeatures = reader.GetString(reader.GetOrdinal("AccessibilityFeatures")),
                    Description = reader.GetString(reader.GetOrdinal("Description"))
                }
            };
        }
    }
}