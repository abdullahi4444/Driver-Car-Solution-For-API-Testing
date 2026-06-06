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
    public class VehicleServices : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public VehicleServices(AppDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: api/vehicleservices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehiclesServicesTypes>>> GetVehicleServices()
        {
            var query = _context.VehicleServices
                .Include(vs => vs.Vehicle)
                .Include(vs => vs.ServiceType)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(vs => vs.ApplicationUserId == userId);
            }

            return await query.ToListAsync();
        }

        // GET: api/vehicleservices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiclesServicesTypes>> GetVehicleService(int id)
        {
            var vehicleService = await _context.VehicleServices
                .Include(vs => vs.Vehicle)
                .Include(vs => vs.ServiceType)
                .FirstOrDefaultAsync(vs => vs.Id == id);

            if (vehicleService == null)
            {
                return NotFound(new { message = $"Vehicle Service with ID {id} not found." });
            }

            if (!User.IsInRole("Admin") && vehicleService.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return vehicleService;
        }

        // GET: api/vehicleservices/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehiclesServicesTypes>>> GetServicesByVehicle(int vehicleId)
        {
            var query = _context.VehicleServices
                .Include(vs => vs.ServiceType)
                .Where(vs => vs.VehicleId == vehicleId);

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(vs => vs.ApplicationUserId == userId);
            }

            var vehicleServices = await query.ToListAsync();

            if (!vehicleServices.Any())
            {
                return NotFound(new { message = $"No services found for vehicle ID {vehicleId}." });
            }

            return vehicleServices;
        }

        // POST: api/vehicleservices
        [HttpPost]
        public async Task<ActionResult<VehiclesServicesTypes>> CreateVehicleService([FromBody] VehiclesServicesTypes vehicleService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify vehicle exists
            var vehicleExists = await _context.Vehicles.AnyAsync(v => v.VehicleId == vehicleService.VehicleId);
            if (!vehicleExists)
            {
                return BadRequest(new { message = $"Vehicle with ID {vehicleService.VehicleId} does not exist." });
            }

            // Verify service type exists
            var serviceTypeExists = await _context.ServiceTypes.AnyAsync(s => s.ServiceTypeId == vehicleService.ServiceTypeId);
            if (!serviceTypeExists)
            {
                return BadRequest(new { message = $"Service Type with ID {vehicleService.ServiceTypeId} does not exist." });
            }

            vehicleService.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.VehicleServices.Add(vehicleService);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicleService), new { id = vehicleService.Id }, vehicleService);
        }

        // PUT: api/vehicleservices/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicleService(int id, [FromBody] VehiclesServicesTypes vehicleService)
        {
            if (id != vehicleService.Id)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingService = await _context.VehicleServices.FindAsync(id);
            if (existingService == null)
            {
                return NotFound(new { message = $"Vehicle Service with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingService, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            // Verify vehicle exists if changed
            if (existingService.VehicleId != vehicleService.VehicleId)
            {
                var vehicleExists = await _context.Vehicles.AnyAsync(v => v.VehicleId == vehicleService.VehicleId);
                if (!vehicleExists)
                {
                    return BadRequest(new { message = $"Vehicle with ID {vehicleService.VehicleId} does not exist." });
                }
            }

            // Verify service type exists if changed
            if (existingService.ServiceTypeId != vehicleService.ServiceTypeId)
            {
                var serviceTypeExists = await _context.ServiceTypes.AnyAsync(s => s.ServiceTypeId == vehicleService.ServiceTypeId);
                if (!serviceTypeExists)
                {
                    return BadRequest(new { message = $"Service Type with ID {vehicleService.ServiceTypeId} does not exist." });
                }
            }

            // Update properties
            existingService.VehicleId = vehicleService.VehicleId;
            existingService.ServiceTypeId = vehicleService.ServiceTypeId;
            existingService.ServiceDate = vehicleService.ServiceDate;
            existingService.Price = vehicleService.Price;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleServiceExists(id))
                {
                    return NotFound(new { message = $"Vehicle Service with ID {id} not found." });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/vehicleservices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleService(int id)
        {
            var vehicleService = await _context.VehicleServices.FindAsync(id);
            if (vehicleService == null)
            {
                return NotFound(new { message = $"Vehicle Service with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, vehicleService, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _context.VehicleServices.Remove(vehicleService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleServiceExists(int id)
        {
            return _context.VehicleServices.Any(e => e.Id == id);
        }
    }
}