using System.ComponentModel.DataAnnotations;

namespace Drive_Car_Solution.Models
{
    public class VehiclesServicesTypes
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
    }
}
