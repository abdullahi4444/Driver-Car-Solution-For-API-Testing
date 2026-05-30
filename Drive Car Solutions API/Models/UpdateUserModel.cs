using System.ComponentModel.DataAnnotations;

namespace Drive_Car_Solutions_API.Models
{
    public class UpdateUserModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string Username { get; set; } = null!;

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Leave empty / null to keep the existing password unchanged.
        /// </summary>
        public string? NewPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = null!;
    }
}
