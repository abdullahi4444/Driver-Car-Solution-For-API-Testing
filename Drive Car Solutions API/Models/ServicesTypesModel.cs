using System.ComponentModel.DataAnnotations;

using Drive_Car_Solutions_API.Models;

namespace Drive_Car_Solution.Models
{
    public class ServicesTypesModel : IOwnedResource
    {
        [Key]
        public int ServiceTypeId { get; set; }

        [Required]
        public string ServiceName { get; set; }

        // Navigation
        public ICollection<VehiclesServicesTypes>? VehicleServices { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}
