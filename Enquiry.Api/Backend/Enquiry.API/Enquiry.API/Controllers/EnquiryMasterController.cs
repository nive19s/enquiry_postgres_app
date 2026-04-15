using Enquiry.Data.Model;
using Enquiry.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Enquiry.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("allowCors")]
    [Authorize]
    public class EnquiryMasterController : ControllerBase
    {
        private readonly IEnquiryService _enquiryService;

        private readonly ILogger<EnquiryMasterController> _logger;

        public EnquiryMasterController(IEnquiryService enquiryService, ILogger<EnquiryMasterController> logger)
        {
            _enquiryService = enquiryService;
            _logger = logger;
        }

        // Helper: Get User ID from Token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            }
            if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException("User ID not found in token");
            return int.Parse(userIdClaim);
        }

        // Helper: Check Admin
        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim?.ToLower() == "admin";
        }

        [HttpGet("GetAllStatus")]
        public async Task<IActionResult> GetEnquiryStatus()
        {
            _logger.LogDebug("[{Class}] {Method}: Fetching all enquiry statuses",
        nameof(EnquiryMasterController), nameof(GetEnquiryStatus));

            try
            {
                var list = await _enquiryService.GetEnquiryStatusAsync();

                _logger.LogInformation("[{Class}] {Method}: Successfully returned {Count} statuses",
            nameof(EnquiryMasterController), nameof(GetEnquiryStatus), list.Count);

                return Ok(list);
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Error fetching statuses",
           nameof(EnquiryMasterController), nameof(GetEnquiryStatus));

                return StatusCode(500, new { message = "Error fetching enquiry statuses"});
            }
            
        }

        [HttpGet("GetAllTypes")]
        public async Task<IActionResult> GetEnquiryType()
        {
            _logger.LogDebug("[{Class}] {Method}: Fetching all enquiry types",
        nameof(EnquiryMasterController), nameof(GetEnquiryType));

            try
            {
                var list = await _enquiryService.GetEnquiryTypeAsync();

                _logger.LogInformation("[{Class}] {Method}: Successfully returned {Count} types",
            nameof(EnquiryMasterController), nameof(GetEnquiryType), list.Count);

                return Ok(list);
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Error fetching types",
             nameof(EnquiryMasterController), nameof(GetEnquiryType));

                return StatusCode(500, new { message = "Error fetching enquiry types"});
            }

        }

        [HttpGet("GetAllEnquiry")]
        public async Task<IActionResult> GetAllEnquiry()
        {
            try
            {

                int userId = GetCurrentUserId();
                bool isAdmin = IsAdmin();
                _logger.LogDebug("[{Class}] {Method}: Attempting to fetch enquiries for UserId: {UserId}, IsAdmin: {IsAdmin}",
                    nameof(EnquiryMasterController), nameof(GetAllEnquiry), userId, isAdmin);



                var list = await _enquiryService.GetAllEnquiryAsync(GetCurrentUserId(), IsAdmin());

                _logger.LogInformation("[{Class}] {Method}: Successfully retrieved {Count} enquiries for UserId: {UserId}",
            nameof(EnquiryMasterController), nameof(GetAllEnquiry), list.Count, userId);

                return Ok(list);
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Failed to get enquiries",
            nameof(EnquiryMasterController), nameof(GetAllEnquiry));

                return StatusCode(500, new { message = "Error fetching enquiries"});
            }
        }
        [HttpPost("CreateNewEnquiry")]
        public async Task<IActionResult> AddNewEnquiry([FromBody] EnquiryModel obj)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogDebug("[{Class}] {Method}: Creating new enquiry for UserId: {UserId}",
                    nameof(EnquiryMasterController), nameof(AddNewEnquiry), userId);

                await _enquiryService.AddNewEnquiryAsync(obj, GetCurrentUserId());

                _logger.LogInformation("[{Class}] {Method}: Enquiry created successfully for UserId: {UserId}",
            nameof(EnquiryMasterController), nameof(AddNewEnquiry), userId);

                return Ok(new { message = "Enquiry created successfully" });
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Failed to create enquiry",
            nameof(EnquiryMasterController), nameof(AddNewEnquiry));

                return StatusCode(500, new { message = "Failed to create enquiry"});
            }
        }
        [HttpPut("UpdateEnquiry")]
        public async Task<IActionResult> Update(EnquiryModel obj)
        {
            try
            {
                bool isAdmin = IsAdmin();
                _logger.LogDebug("[{Class}] {Method}: Updating EnquiryId: {EnquiryId} (IsAdmin: {IsAdmin})",
                    nameof(EnquiryMasterController), nameof(Update), obj.enquiryId, isAdmin);

                await _enquiryService.UpdateEnquiryAsync(obj, IsAdmin());

                _logger.LogInformation("[{Class}] {Method}: Successfully updated EnquiryId: {EnquiryId}",
            nameof(EnquiryMasterController), nameof(Update), obj.enquiryId);

                return Ok(new { message = "Enquiry updated successfully" });
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Failed to update EnquiryId: {EnquiryId}",
            nameof(EnquiryMasterController), nameof(Update), obj.enquiryId);

                return StatusCode(500, new { message = "Failed to update enquiry"});
            }
        }
        [HttpDelete("DeleteEnquiryById/{id}")]
        public async Task<IActionResult> DeleteEnquiryById(int id)
        {
            try
            {
                _logger.LogDebug("[{Class}] {Method}: Deleting EnquiryId: {EnquiryId}",
            nameof(EnquiryMasterController), nameof(DeleteEnquiryById), id);

                await _enquiryService.DeleteEnquiryAsync(id);

                _logger.LogInformation("[{Class}] {Method}: Successfully deleted EnquiryId: {EnquiryId}",
            nameof(EnquiryMasterController), nameof(DeleteEnquiryById), id);

                return Ok(new { message = "Enquiry deleted successfully" });
            }
            catch (Exception ex)
            {
                //Log the error
                _logger.LogError(ex, "[{Class}] {Method}: Failed to delete EnquiryId: {EnquiryId}",
           nameof(EnquiryMasterController), nameof(DeleteEnquiryById), id);

                return StatusCode(500, new { message = "Failed to delete"});
            }
        }
    }
}