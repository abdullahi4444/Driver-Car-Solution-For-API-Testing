using System.ComponentModel.DataAnnotations;

using Drive_Car_Solutions_API.Models;

namespace Drive_Car_Solution.Models
{
    public class VehiclesServicesTypes : IOwnedResource
    {
        [Key]
        public int Id { get; set; }

        // FK Vehicle
        public int VehicleId { get; set; }
        public VehiclesModel? Vehicle { get; set; }

        // FK Service
        public int ServiceTypeId { get; set; }
        public ServicesTypesModel? ServiceType { get; set; }

        public DateTime ServiceDate { get; set; }

        public decimal Price { get; set; }

        public string? ApplicationUserId { get; set; }
    }
}
