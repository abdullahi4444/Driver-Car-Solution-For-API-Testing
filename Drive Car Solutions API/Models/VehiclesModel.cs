using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drive_Car_Solution.Models
{
    public class VehiclesModel
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        public string CarName { get; set; }

        public string? PlateNumber { get; set; }

        public string? Model { get; set; }

        // FK
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public CustomersModel? Customer { get; set; }

        // Navigation
        public ICollection<VehiclesServicesTypes>? VehicleServices { get; set; }
    }
}
