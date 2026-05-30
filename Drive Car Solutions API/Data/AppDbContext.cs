using Drive_Car_Solution.Models;
using Drive_Car_Solutions_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Drive_Car_Solutions_API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }

        public DbSet<CustomersModel> Customers { get; set; } = null!;

        public DbSet<VehiclesModel> Vehicles { get; set; } = null!;

        public DbSet<ServicesTypesModel> ServiceTypes { get; set; } = null!;

        public DbSet<VehiclesServicesTypes> VehicleServices { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // Seed Identity Roles
            // =========================

            modelBuilder.Entity<IdentityRole>().HasData(

                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "1"
                },

                new IdentityRole
                {
                    Id = "2",
                    Name = "Staff",
                    NormalizedName = "STAFF",
                    ConcurrencyStamp = "2"
                },

                new IdentityRole
                {
                    Id = "3",
                    Name = "User",
                    NormalizedName = "USER",
                    ConcurrencyStamp = "3"
                }

            );

            // =========================
            // Seed Service Types
            // =========================

            modelBuilder.Entity<ServicesTypesModel>().HasData(

                new ServicesTypesModel
                {
                    ServiceTypeId = 1,
                    ServiceName = "Oil Change"
                },

                new ServicesTypesModel
                {
                    ServiceTypeId = 2,
                    ServiceName = "Car Wash"
                },

                new ServicesTypesModel
                {
                    ServiceTypeId = 3,
                    ServiceName = "Tire Alignment"
                },

                new ServicesTypesModel
                {
                    ServiceTypeId = 4,
                    ServiceName = "Brake Inspection"
                },

                new ServicesTypesModel
                {
                    ServiceTypeId = 5,
                    ServiceName = "Battery Replacement"
                },

                new ServicesTypesModel
                {
                    ServiceTypeId = 6,
                    ServiceName = "Diagnostic Scan"
                }

            );
        }
    }
}