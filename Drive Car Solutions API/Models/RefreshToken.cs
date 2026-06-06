using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drive_Car_Solutions_API.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsRevoked { get; set; } = false;

        [ForeignKey(nameof(ApplicationUser))]
        public string ApplicationUserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
