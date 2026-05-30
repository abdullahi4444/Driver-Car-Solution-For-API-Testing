using System.ComponentModel.DataAnnotations;

namespace Drive_Car_Solution.Models
{
    public class CustomersModel
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
    }
}
