using Drive_Car_Solution.Models;
using Drive_Car_Solutions_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Drive_Car_Solutions_API.Authorization;
using System.Security.Claims;

namespace Drive_Car_Solutions_API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Vehicles : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public Vehicles(AppDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: api/vehicles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiclesModel>>> GetVehicles()
        {
            var query = _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleServices)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(v => v.ApplicationUserId == userId);
            }

            return await query.ToListAsync();
        }

        // GET: api/vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiclesModel>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleServices)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
            {
                return NotFound(new { message = $"Vehicle with ID {id} not found." });
            }

            if (!User.IsInRole("Admin") && vehicle.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return vehicle;
        }

        // GET: api/vehicles/customer/5
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<VehiclesModel>>> GetVehiclesByCustomer(int customerId)
        {
            var query = _context.Vehicles
                .Include(v => v.VehicleServices)
                .Where(v => v.CustomerId == customerId);

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(v => v.ApplicationUserId == userId);
            }

            var vehicles = await query.ToListAsync();

            if (!vehicles.Any())
            {
                return NotFound(new { message = $"No vehicles found for customer ID {customerId}." });
            }

            return vehicles;
        }

        // POST: api/vehicles
        [HttpPost]
        public async Task<ActionResult<VehiclesModel>> CreateVehicle([FromBody] VehiclesModel vehicle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify customer exists
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == vehicle.CustomerId);
            if (!customerExists)
            {
                return BadRequest(new { message = $"Customer with ID {vehicle.CustomerId} does not exist." });
            }

            vehicle.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.VehicleId }, vehicle);
        }

        // PUT: api/vehicles/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] VehiclesModel vehicle)
        {
            if (id != vehicle.VehicleId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingVehicle = await _context.Vehicles.FindAsync(id);
            if (existingVehicle == null)
            {
                return NotFound(new { message = $"Vehicle with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingVehicle, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            // Verify customer exists if CustomerId changed
            if (existingVehicle.CustomerId != vehicle.CustomerId)
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == vehicle.CustomerId);
                if (!customerExists)
                {
                    return BadRequest(new { message = $"Customer with ID {vehicle.CustomerId} does not exist." });
                }
            }

            // Update properties
            existingVehicle.CarName = vehicle.CarName;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.PlateNumber = vehicle.PlateNumber;
            existingVehicle.CustomerId = vehicle.CustomerId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id))
                {
                    return NotFound(new { message = $"Vehicle with ID {id} not found." });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/vehicles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound(new { message = $"Vehicle with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, vehicle, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.VehicleId == id);
        }
    }
}