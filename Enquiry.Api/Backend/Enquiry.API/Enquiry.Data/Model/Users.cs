using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Enquiry.Data.Model
{
    [Table("Users")]
    public class Users
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int userId { get; set; }
        [Required]
        public string fullName { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string? password { get; set; } = string.Empty;

        public string role {  get; set; } = string.Empty;

        public DateTime createdDate { get; set; }

        public string? GoogleId { get; set; } = string.Empty;

        public string? MicrosoftId { get; set; } = string.Empty;

        public string? ProfileImage { get; set; } = string.Empty;
    }
}
