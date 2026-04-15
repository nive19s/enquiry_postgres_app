using Enquiry.Data.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;



namespace Enquiry.Data.Services
{
        public class EnquiryService : IEnquiryService
        {
            private readonly string _connectionString;
            private readonly ILookupService _lookupService;
            private readonly ILogger<EnquiryService> _logger;


        public EnquiryService(IOptions<DatabaseSettings> dbSettings, ILookupService lookupService, ILogger<EnquiryService> logger)
            {
            _connectionString = dbSettings.Value.ConnectionString;
            _lookupService = lookupService;
            _logger = logger;
        }


        //Get Enquiry Status from Lookup Service
        public async Task<List<EnquiryStatus>> GetEnquiryStatusAsync()
        {
            // Use the Singleton Service instead of SQL!
            return await _lookupService.GetEnquiryStatusAsync();
        }


        //Get Enquiry Type from Lookup Service
        public async Task<List<EnquiryType>> GetEnquiryTypeAsync()
        {
            // Use the Singleton Service instead of SQL!
            return await _lookupService.GetEnquiryTypeAsync();
        }


        //Get All Enquiries
        public async Task<List<EnquiryModel>> GetAllEnquiryAsync(int userId, bool isAdmin)
        {
            _logger.LogDebug("[{Class}] {Method}: Fetching enquiries for UserId: {UserId}, IsAdmin: {IsAdmin}",
                nameof(EnquiryService), nameof(GetAllEnquiryAsync), userId, isAdmin);

            try
            {
                var list = new List<EnquiryModel>();
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    _logger.LogDebug("[{Class}] {Method}: Executing get_all_enquiries SQL",
                        nameof(EnquiryService), nameof(GetAllEnquiryAsync));

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_all_enquiries(@p_userid, @p_isadmin)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_userid", userId);
                        cmd.Parameters.AddWithValue("p_isadmin", isAdmin);

                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                list.Add(new EnquiryModel
                                {
                                    enquiryId = (int)reader["enquiryid"],
                                    userId = (int)reader["userid"],
                                    enquiryTypeId = (int)reader["enquirytypeid"],
                                    enquiryStatusId = (int)reader["enquirystatusid"],
                                    customerName = reader["customername"].ToString(),
                                    mobileNo = reader["mobileno"].ToString(),
                                    email = reader["email"].ToString(),
                                    message = reader["message"].ToString(),
                                    resolution = reader["resolution"] != DBNull.Value ? reader["resolution"].ToString() : null,
                                    createdDate = (DateTime)reader["createddate"]
                                });
                            }
                        }
                    }
                }

                _logger.LogInformation("[{Class}] {Method}: Successfully retrieved {Count} enquiries for UserId: {UserId}",
                    nameof(EnquiryService), nameof(GetAllEnquiryAsync), list.Count, userId);

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Error encountered for UserId: {UserId}",
                    nameof(EnquiryService), nameof(GetAllEnquiryAsync), userId);
                throw;
            }
        }

        //Add New Enquiry
        public async Task<bool> AddNewEnquiryAsync(EnquiryModel obj, int userId)
        {
            // LogDebug: Method Entry (Includes UserId)
            _logger.LogDebug("[{Class}] {Method}: Attempting to create new enquiry for UserId: {UserId}",
                nameof(EnquiryService), nameof(AddNewEnquiryAsync), userId);

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // LogDebug: Before Database Call
                    _logger.LogDebug("[{Class}] {Method}: Executing create_enquiry PostgreSQL function",
                        nameof(EnquiryService), nameof(AddNewEnquiryAsync));

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT create_enquiry(@p_userid, @p_enquirytypeid, @p_enquirystatusid, @p_customername, @p_mobileno, @p_email, @p_message, @p_resolution)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_userid", userId);
                        cmd.Parameters.AddWithValue("p_enquirytypeid", obj.enquiryTypeId);
                        cmd.Parameters.AddWithValue("p_enquirystatusid", obj.enquiryStatusId == 0 ? 1 : obj.enquiryStatusId);
                        cmd.Parameters.AddWithValue("p_customername", obj.customerName);
                        cmd.Parameters.AddWithValue("p_mobileno", obj.mobileNo);
                        cmd.Parameters.AddWithValue("p_email", obj.email);
                        cmd.Parameters.AddWithValue("p_message", obj.message);
                        cmd.Parameters.AddWithValue("p_resolution", (object)obj.resolution ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();

                        // LogInformation: Successful Outcome
                        _logger.LogInformation("[{Class}] {Method}: Successfully created enquiry for UserId: {UserId}",
                            nameof(EnquiryService), nameof(AddNewEnquiryAsync), userId);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // LogError: Exception object + Structured Identifiers
                _logger.LogError(ex, "[{Class}] {Method}: Failed to create enquiry for UserId: {UserId}",
                    nameof(EnquiryService), nameof(AddNewEnquiryAsync), userId);

                throw; // Let the controller handle the response
            }
        }

        //Update Enquiry
        public async Task<bool> UpdateEnquiryAsync(EnquiryModel obj, bool isAdmin)
        {
            // LogDebug: Entry point
            _logger.LogDebug("[{Class}] {Method}: Entering with EnquiryId: {EnquiryId}, IsAdmin: {IsAdmin}",
                nameof(EnquiryService), nameof(UpdateEnquiryAsync), obj.enquiryId, isAdmin);

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    // LogDebug: Before DB call
                    _logger.LogDebug("[{Class}] {Method}: Executing update_enquiry for ID: {EnquiryId}",
                        nameof(EnquiryService), nameof(UpdateEnquiryAsync), obj.enquiryId);

                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT update_enquiry(@p_enquiryid, @p_resolution, @p_enquirystatusid, @p_isadmin, @p_customername, @p_mobileno, @p_email, @p_message, @p_enquirytypeid)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_enquiryid", obj.enquiryId);
                        cmd.Parameters.AddWithValue("p_resolution", (object)obj.resolution ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("p_enquirystatusid", obj.enquiryStatusId);
                        cmd.Parameters.AddWithValue("p_isadmin", isAdmin);

                        // Add your existing conditional parameters here...
                        if (isAdmin)
                        {
                            cmd.Parameters.AddWithValue("p_customername", obj.customerName);
                            cmd.Parameters.AddWithValue("p_mobileno", obj.mobileNo);
                            cmd.Parameters.AddWithValue("p_email", obj.email);
                            cmd.Parameters.AddWithValue("p_message", obj.message);
                            cmd.Parameters.AddWithValue("p_enquirytypeid", obj.enquiryTypeId);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("p_customername", DBNull.Value);
                            cmd.Parameters.AddWithValue("p_mobileno", DBNull.Value);
                            cmd.Parameters.AddWithValue("p_email", DBNull.Value);
                            cmd.Parameters.AddWithValue("p_message", DBNull.Value);
                            cmd.Parameters.AddWithValue("p_enquirytypeid", 0);
                        }

                        await cmd.ExecuteNonQueryAsync();

                        // LogInformation: Success
                        _logger.LogInformation("[{Class}] {Method}: Successfully updated EnquiryId: {EnquiryId}",
                            nameof(EnquiryService), nameof(UpdateEnquiryAsync), obj.enquiryId);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                // LogError: With context
                _logger.LogError(ex, "[{Class}] {Method}: Error encountered for EnquiryId: {EnquiryId}",
                    nameof(EnquiryService), nameof(UpdateEnquiryAsync), obj.enquiryId);
                throw;
            }
        }

        //  delete Enquiry
        public async Task<bool> DeleteEnquiryAsync(int enquiryId)
        {
            _logger.LogDebug("[{Class}] {Method}: Attempting to delete EnquiryId: {EnquiryId}",
                nameof(EnquiryService), nameof(DeleteEnquiryAsync), enquiryId);

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT delete_enquiry(@p_enquiryid)", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("p_enquiryid", enquiryId);

                        await cmd.ExecuteNonQueryAsync();

                        _logger.LogInformation("[{Class}] {Method}: Successfully deleted EnquiryId: {EnquiryId}",
                            nameof(EnquiryService), nameof(DeleteEnquiryAsync), enquiryId);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Error deleting EnquiryId: {EnquiryId}",
                    nameof(EnquiryService), nameof(DeleteEnquiryAsync), enquiryId);
                throw;
            }
        }

    }
}
