using HotelBookingAPI.DTOs.CancellationDTOs;
using HotelBookingAPI.Models;
using HotelBookingAPI.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
namespace HotelBookingAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CancellationController : ControllerBase
    {
        private readonly ILogger<CancellationController> _logger;
        private readonly CancellationRepository _cancellationRepository;
        public CancellationController(ILogger<CancellationController> logger, CancellationRepository cancellationRepository)
        {
            _logger = logger;
            _cancellationRepository = cancellationRepository;
        }
        [HttpGet("GetCancellationPolicies")]
        public async Task<APIResponse<CancellationPoliciesResponseDTO>> GetCancellationPolicies()
        {
            _logger.LogInformation("Request received for GetCancellationPolicies");
            try
            {
                var result = await _cancellationRepository.GetCancellationPoliciesAsync();
                return new APIResponse<CancellationPoliciesResponseDTO>(result, "Cancellation policies retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve cancellation policies");
                return new APIResponse<CancellationPoliciesResponseDTO>(HttpStatusCode.InternalServerError, "Failed to retrieve cancellation policies", ex.Message);
            }
        }
        [HttpPost("CalculateCancellationCharges")]
        public async Task<APIResponse<CalculateCancellationChargesResponseDTO>> CalculateCancellationCharges([FromBody] CalculateCancellationChargesRequestDTO model)
        {
            _logger.LogInformation("Request Received for CalculateCancellationCharges: {@CalculateCancellationChargesRequestDTO}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body for CalculateCancellationCharges");
                return new APIResponse<CalculateCancellationChargesResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
            }
            try
            {
                var result = await _cancellationRepository.CalculateCancellationChargesAsync(model);
                if (result.Status)
                {
                    return new APIResponse<CalculateCancellationChargesResponseDTO>(result, "Cancellation charges calculated successfully.");
                }
                return new APIResponse<CalculateCancellationChargesResponseDTO>(HttpStatusCode.BadRequest, "Failed to calculate cancellation charges", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate cancellation charges");
                return new APIResponse<CalculateCancellationChargesResponseDTO>(HttpStatusCode.InternalServerError, "Failed to calculate cancellation charges", ex.Message);
            }
        }
        [HttpPost("CreateCancellationRequest")]
        public async Task<APIResponse<CreateCancellationResponseDTO>> CreateCancellationRequest([FromBody] CreateCancellationRequestDTO model)
        {
            _logger.LogInformation("Request Received for CreateCancellationRequest: {@CreateCancellationRequestDTO}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body for CreateCancellationRequest");
                return new APIResponse<CreateCancellationResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
            }
            try
            {
                var result = await _cancellationRepository.CreateCancellationRequestAsync(model);
                if (result.Status)
                {
                    return new APIResponse<CreateCancellationResponseDTO>(result, "Cancellation request created successfully.");
                }
                return new APIResponse<CreateCancellationResponseDTO>(HttpStatusCode.BadRequest, "Failed to create cancellation request", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cancellation request");
                return new APIResponse<CreateCancellationResponseDTO>(HttpStatusCode.InternalServerError, "Failed to create cancellation request", ex.Message);
            }
        }
        [HttpPost("GetAllCancellations")]
        public async Task<APIResponse<AllCancellationsResponseDTO>> GetAllCancellations([FromBody] AllCancellationsRequestDTO model)
        {
            _logger.LogInformation("Request Received for GetAllCancellations: {@AllCancellationsRequestDTO}", model);
            try
            {
                var result = await _cancellationRepository.GetAllCancellationsAsync(model);
                return new APIResponse<AllCancellationsResponseDTO>(result, "All cancellations retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all cancellations");
                return new APIResponse<AllCancellationsResponseDTO>(HttpStatusCode.InternalServerError, "Failed to retrieve all cancellations", ex.Message);
            }
        }
        [HttpPost("ReviewCancellationRequest")]
        public async Task<APIResponse<ReviewCancellationResponseDTO>> ReviewCancellationRequest([FromBody] ReviewCancellationRequestDTO model)
        {
            _logger.LogInformation("Request Received for ReviewCancellationRequest: {@ReviewCancellationRequestDTO}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body for ReviewCancellationRequest");
                return new APIResponse<ReviewCancellationResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
            }
            try
            {
                var result = await _cancellationRepository.ReviewCancellationRequestAsync(model);
                if (result.Status)
                {
                    return new APIResponse<ReviewCancellationResponseDTO>(result, "Cancellation request reviewed successfully.");
                }
                return new APIResponse<ReviewCancellationResponseDTO>(HttpStatusCode.BadRequest, "Failed to review cancellation request", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to review cancellation request");
                return new APIResponse<ReviewCancellationResponseDTO>(HttpStatusCode.InternalServerError, "Failed to review cancellation request", ex.Message);
            }
        }
        [HttpGet("CancellationsForRefund")]
        public async Task<APIResponse<CancellationForRefundResponseDTO>> GetCancellationsForRefund()
        {
            _logger.LogInformation("Request Received for GetCancellationsForRefund");
            try
            {
                var result = await _cancellationRepository.GetCancellationsForRefundAsync();
                if (result == null || !result.CancellationsToRefund.Any())
                    return new APIResponse<CancellationForRefundResponseDTO>(HttpStatusCode.NotFound, "No cancellations found that need refunding.");
                return new APIResponse<CancellationForRefundResponseDTO>(result, "All cancellations to be Refund retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all cancellations to be refunded");
                return new APIResponse<CancellationForRefundResponseDTO>(HttpStatusCode.InternalServerError, "Failed to retrieve all cancellations to be refunded", ex.Message);
            }
        }
        [HttpPost("ProcessRefund")]
        public async Task<APIResponse<ProcessRefundResponseDTO>> ProcessRefund([FromBody] ProcessRefundRequestDTO model)
        {
            _logger.LogInformation("Request Received for ProcessRefund: {@ProcessRefundRequestDTO}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body for ProcessRefund");
                return new APIResponse<ProcessRefundResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
            }
            try
            {
                var result = await _cancellationRepository.ProcessRefundAsync(model);
                if (result.Status)
                {
                    return new APIResponse<ProcessRefundResponseDTO>(result, "Refund processed successfully.");
                }
                return new APIResponse<ProcessRefundResponseDTO>(HttpStatusCode.BadRequest, "Failed to process refund", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process refund");
                return new APIResponse<ProcessRefundResponseDTO>(HttpStatusCode.InternalServerError, "Failed to process refund", ex.Message);
            }
        }
        [HttpPost("UpdateRefundStatus")]
        public async Task<APIResponse<UpdateRefundStatusResponseDTO>> UpdateRefundStatus([FromBody] UpdateRefundStatusRequestDTO model)
        {
            _logger.LogInformation("Request Received for UpdateRefundStatus: {@UpdateRefundStatusRequestDTO}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Invalid Data in the Request Body for UpdateRefundStatus");
                return new APIResponse<UpdateRefundStatusResponseDTO>(HttpStatusCode.BadRequest, "Invalid Data in the Request Body");
            }
            try
            {
                var result = await _cancellationRepository.UpdateRefundStatusAsync(model);
                if (result.Status)
                {
                    return new APIResponse<UpdateRefundStatusResponseDTO>(result, "Refund status updated successfully.");
                }
                return new APIResponse<UpdateRefundStatusResponseDTO>(HttpStatusCode.BadRequest, "Failed to update refund status", result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update refund status");
                return new APIResponse<UpdateRefundStatusResponseDTO>(HttpStatusCode.InternalServerError, "Failed to update refund status", ex.Message);
            }
        }
    }
}