using Enquiry.Data.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace Enquiry.Data.Services
{

    //Lazy<T> Singleton Included
    
    public class LookupService : ILookupService
    {
        private readonly string _connectionString;
        private readonly ILogger<LookupService> _logger;

        // Define Lazy variables
        // These hold the "Promise" of data, not the data itself yet.
        private readonly Lazy<Task<List<EnquiryStatus>>> _lazyStatus;
        private readonly Lazy<Task<List<EnquiryType>>> _lazyType;

        public LookupService(IOptions<DatabaseSettings> dbSettings, ILogger<LookupService> logger)
        {
            _connectionString = dbSettings.Value.ConnectionString;
            _logger = logger;

            // Initialize Lazy objects
            // We tell it: "When someone asks for the value, run this function."
            _lazyStatus = new Lazy<Task<List<EnquiryStatus>>>(() => FetchStatusFromDb());
            _lazyType = new Lazy<Task<List<EnquiryType>>>(() => FetchTypeFromDb());
        }

        public Task<List<EnquiryStatus>> GetEnquiryStatusAsync()
        {
            _logger.LogDebug("[{Class}] {Method}: Returning statuses from cache/lazy value",
       nameof(LookupService), nameof(GetEnquiryStatusAsync));
            return _lazyStatus.Value;
        }

        public Task<List<EnquiryType>> GetEnquiryTypeAsync()
        {
            _logger.LogDebug("[{Class}] {Method}: Returning types from cache/lazy value",
        nameof(LookupService), nameof(GetEnquiryTypeAsync));
            return _lazyType.Value;
        }

        // Private Helper Methods (The actual work)
        private async Task<List<EnquiryStatus>> FetchStatusFromDb()
        {

            // LogInformation: This proves the singleton pattern is working!
            _logger.LogInformation("[{Class}] {Method}: Initializing Enquiry Statuses from Database (First call)",
                nameof(LookupService), nameof(FetchStatusFromDb));

            try
            {
                var list = new List<EnquiryStatus>();
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_all_status()", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                list.Add(new EnquiryStatus
                                {
                                    statusId = (int)reader["statusid"],
                                    status = reader["status"].ToString()
                                });
                            }
                        }
                    }
                }
                return list;
                }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Class}] {Method}: Failed to load statuses from database",
                nameof(LookupService), nameof(FetchStatusFromDb));
                throw;
            }
        }

        private async Task<List<EnquiryType>> FetchTypeFromDb()
        {
            _logger.LogInformation("[{Class}] {Method}: Initializing Enquiry Types from Database (First call)",
        nameof(LookupService), nameof(FetchTypeFromDb));
            try
            {
                var list = new List<EnquiryType>();
                using (NpgsqlConnection conn = new NpgsqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM get_all_types()", conn))
                    {
                        cmd.CommandType = CommandType.Text;
                        using (NpgsqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                list.Add(new EnquiryType
                                {
                                    typeId = (int)reader["typeid"],
                                    typeName = reader["typename"].ToString()
                                });
                            }
                        }
                    }
                }
                return list;
            }
            catch(Exception ex)
            {
                // LogError: With Exception object and Class/Method context
                _logger.LogError(ex, "[{Class}] {Method}: Failed to load types from database",
                    nameof(LookupService), nameof(FetchTypeFromDb));
                throw;
            }
        }
    }
}