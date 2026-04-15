using Enquiry.Data.Model;

namespace Enquiry.Data.Services
{
    public interface ILookupService
    {
        Task<List<EnquiryStatus>> GetEnquiryStatusAsync();
        Task<List<EnquiryType>> GetEnquiryTypeAsync();
    }
}
