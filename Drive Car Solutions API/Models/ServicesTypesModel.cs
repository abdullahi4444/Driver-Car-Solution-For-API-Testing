using System.ComponentModel.DataAnnotations;

namespace Drive_Car_Solution.Models
{
    public class ServicesTypesModel
    {
        [Key]
        public int ServiceTypeId { get; set; }

        [Required]
        public string ServiceName { get; set; }

        // Navigation
        public ICollection<VehiclesServicesTypes>? VehicleServices { get; set; }
    }
}
