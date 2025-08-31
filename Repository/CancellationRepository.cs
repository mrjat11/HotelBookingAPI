using HotelBookingAPI.Connection;
using HotelBookingAPI.DTOs.CancellationDTOs;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HotelBookingAPI.Repository
{
    public class CancellationRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;
        public CancellationRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<CancellationPoliciesResponseDTO> GetCancellationPoliciesAsync()
        {
            var response = new CancellationPoliciesResponseDTO
            {
                Policies = new List<CancellationPolicyDTO>()
            };
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spGetCancellationPolicies", connection);
                command.CommandType = CommandType.StoredProcedure;
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        response.Policies.Add(new CancellationPolicyDTO
                        {
                            PolicyID = reader.GetInt32(reader.GetOrdinal("PolicyID")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            CancellationChargePercentage = reader.GetDecimal(reader.GetOrdinal("CancellationChargePercentage")),
                            MinimumCharge = reader.GetDecimal(reader.GetOrdinal("MinimumCharge")),
                            EffectiveFromDate = reader.GetDateTime(reader.GetOrdinal("EffectiveFromDate")),
                            EffectiveToDate = reader.GetDateTime(reader.GetOrdinal("EffectiveToDate"))
                        });
                    }
                }
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<CalculateCancellationChargesResponseDTO> CalculateCancellationChargesAsync(CalculateCancellationChargesRequestDTO request)
        {
            var response = new CalculateCancellationChargesResponseDTO();
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spCalculateCancellationCharges", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ReservationID", request.ReservationID);
                command.Parameters.AddWithValue("@RoomsCancelled", CreateRoomsCancelledTable(request.RoomsCancelled));
                var totalCostParam = new SqlParameter("@TotalCost", SqlDbType.Decimal) { Precision = 10, Scale = 2, Direction = ParameterDirection.Output };
                var cancellationChargeParam = new SqlParameter("@CancellationCharge", SqlDbType.Decimal) { Precision = 10, Scale = 2, Direction = ParameterDirection.Output };
                var cancellationPercentageParam = new SqlParameter("@CancellationPercentage", SqlDbType.Decimal) { Precision = 10, Scale = 2, Direction = ParameterDirection.Output };
                var policyDescriptionParam = new SqlParameter("@PolicyDescription", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                command.Parameters.Add(totalCostParam);
                command.Parameters.Add(cancellationChargeParam);
                command.Parameters.Add(cancellationPercentageParam);
                command.Parameters.Add(policyDescriptionParam);
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
                if (response.Status)
                {
                    response.TotalCost = (decimal)totalCostParam.Value;
                    response.CancellationCharge = (decimal)cancellationChargeParam.Value;
                    response.CancellationPercentage = (decimal)cancellationPercentageParam.Value;
                    response.PolicyDescription = policyDescriptionParam.Value as string;
                }
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<CreateCancellationResponseDTO> CreateCancellationRequestAsync(CreateCancellationRequestDTO request)
        {
            var response = new CreateCancellationResponseDTO();
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spCreateCancellationRequest", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserID", request.UserID);
                command.Parameters.AddWithValue("@ReservationID", request.ReservationID);
                command.Parameters.AddWithValue("@RoomsCancelled", CreateRoomsCancelledTable(request.RoomsCancelled));
                command.Parameters.AddWithValue("@CancellationReason", request.CancellationReason);
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var cancellationRequestIDParam = new SqlParameter("@CancellationRequestID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                command.Parameters.Add(cancellationRequestIDParam);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
                if (response.Status)
                {
                    response.CancellationId = (int)command.Parameters["@CancellationRequestID"].Value;
                }
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<AllCancellationsResponseDTO> GetAllCancellationsAsync(AllCancellationsRequestDTO request)
        {
            var response = new AllCancellationsResponseDTO
            {
                Cancellations = new List<CancellationRequestDTO>()
            };
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spGetAllCancellations", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Status", request.Status ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DateFrom", request.DateFrom ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DateTo", request.DateTo ?? (object)DBNull.Value);
                var statusOutParam = new SqlParameter("@StatusOut", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageOutParam = new SqlParameter("@MessageOut", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(statusOutParam);
                command.Parameters.Add(messageOutParam);
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        response.Cancellations.Add(new CancellationRequestDTO
                        {
                            CancellationRequestID = reader.GetInt32(reader.GetOrdinal("CancellationRequestID")),
                            ReservationID = reader.GetInt32(reader.GetOrdinal("ReservationID")),
                            UserID = reader.GetInt32(reader.GetOrdinal("UserID")),
                            CancellationType = reader.GetString(reader.GetOrdinal("CancellationType")),
                            RequestedOn = reader.GetDateTime(reader.GetOrdinal("RequestedOn")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        });
                    }
                }
                response.Status = (bool)statusOutParam.Value;
                response.Message = messageOutParam.Value as string;
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<ReviewCancellationResponseDTO> ReviewCancellationRequestAsync(ReviewCancellationRequestDTO request)
        {
            var response = new ReviewCancellationResponseDTO();
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spReviewCancellationRequest", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CancellationRequestID", request.CancellationRequestID);
                command.Parameters.AddWithValue("@AdminUserID", request.AdminUserID);
                command.Parameters.AddWithValue("@ApprovalStatus", request.ApprovalStatus);
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<CancellationForRefundResponseDTO> GetCancellationsForRefundAsync()
        {
            var response = new CancellationForRefundResponseDTO
            {
                CancellationsToRefund = new List<CancellationForRefundDTO>()
            };
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spGetCancellationsForRefund", connection);
                command.CommandType = CommandType.StoredProcedure;
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        response.CancellationsToRefund.Add(new CancellationForRefundDTO
                        {
                            CancellationRequestID = reader.GetInt32("CancellationRequestID"),
                            ReservationID = reader.GetInt32("ReservationID"),
                            UserID = reader.GetInt32("UserID"),
                            CancellationType = reader.GetString("CancellationType"),
                            RequestedOn = reader.GetDateTime("RequestedOn"),
                            Status = reader.GetString("Status"),
                            RefundID = reader.GetInt32("RefundID"),
                            RefundStatus = reader.GetString("RefundStatus")
                        });
                    }
                }
                response.Status = true;
                response.Message = $"Success";
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<ProcessRefundResponseDTO> ProcessRefundAsync(ProcessRefundRequestDTO request)
        {
            var response = new ProcessRefundResponseDTO();
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spProcessRefund", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@CancellationRequestID", request.CancellationRequestID);
                command.Parameters.AddWithValue("@ProcessedByUserID", request.ProcessedByUserID);
                command.Parameters.AddWithValue("@RefundMethodID", request.RefundMethodID);
                var refundIDParam = new SqlParameter("@RefundID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(refundIDParam);
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
                if (response.Status)
                {
                    response.RefundID = (int)command.Parameters["@RefundID"].Value;
                }
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        public async Task<UpdateRefundStatusResponseDTO> UpdateRefundStatusAsync(UpdateRefundStatusRequestDTO request)
        {
            var response = new UpdateRefundStatusResponseDTO();
            try
            {
                using var connection = _connectionFactory.CreateConnection();
                using var command = new SqlCommand("spUpdateRefundStatus", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@RefundID", request.RefundID);
                command.Parameters.AddWithValue("@NewRefundStatus", request.NewRefundStatus);
                var statusParam = new SqlParameter("@Status", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var messageParam = new SqlParameter("@Message", SqlDbType.NVarChar, 255) { Direction = ParameterDirection.Output };
                command.Parameters.Add(statusParam);
                command.Parameters.Add(messageParam);
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
                response.Status = (bool)statusParam.Value;
                response.Message = messageParam.Value as string;
            }
            catch (SqlException ex)
            {
                response.Status = false;
                response.Message = $"Database error occurred: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = $"An error occurred: {ex.Message}";
            }
            return response;
        }
        private DataTable CreateRoomsCancelledTable(IEnumerable<int> roomIds)
        {
            var table = new DataTable();
            table.Columns.Add("RoomID", typeof(int));
            foreach (var id in roomIds)
            {
                table.Rows.Add(id);
            }
            return table;
        }
    }
}