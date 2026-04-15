using Enquiry.Data.Model;

namespace Enquiry.Data.Services
{
    public interface IEnquiryService
    {
        Task<List<EnquiryStatus>> GetEnquiryStatusAsync();
        Task<List<EnquiryType>> GetEnquiryTypeAsync();
        Task<List<EnquiryModel>> GetAllEnquiryAsync(int userId, bool isAdmin);
        Task<bool> AddNewEnquiryAsync(EnquiryModel obj, int userId);
        Task<bool> UpdateEnquiryAsync(EnquiryModel obj, bool isAdmin);
        Task<bool> DeleteEnquiryAsync(int enquiryId);
    }
}
