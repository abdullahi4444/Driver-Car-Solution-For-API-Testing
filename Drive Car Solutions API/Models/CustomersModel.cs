using System.ComponentModel.DataAnnotations;

using Drive_Car_Solutions_API.Models;

namespace Drive_Car_Solution.Models
{
    public class CustomersModel : IOwnedResource
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        public string? Address { get; set; }

        // Navigation
        public ICollection<VehiclesModel>? Vehicles { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}
