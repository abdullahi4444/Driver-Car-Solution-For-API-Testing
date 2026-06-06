using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Drive_Car_Solutions_API.Models;

namespace Drive_Car_Solution.Models
{
    public class VehiclesModel : IOwnedResource
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

        public string? ApplicationUserId { get; set; }
    }
}
