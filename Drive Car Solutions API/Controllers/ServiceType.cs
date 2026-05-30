using Drive_Car_Solution.Models;
using Drive_Car_Solutions_API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drive_Car_Solutions_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTypes : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceTypes(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/servicetypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServicesTypesModel>>> GetServiceTypes()
        {
            return await _context.ServiceTypes
                .Include(s => s.VehicleServices)
                .ToListAsync();
        }

        // GET: api/servicetypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServicesTypesModel>> GetServiceType(int id)
        {
            var serviceType = await _context.ServiceTypes
                .Include(s => s.VehicleServices)
                .FirstOrDefaultAsync(s => s.ServiceTypeId == id);

            if (serviceType == null)
            {
                return NotFound(new { message = $"Service Type with ID {id} not found." });
            }

            return serviceType;
        }

        // POST: api/servicetypes
        [HttpPost]
        public async Task<ActionResult<ServicesTypesModel>> CreateServiceType([FromBody] ServicesTypesModel serviceType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if service name already exists
            var exists = await _context.ServiceTypes
                .AnyAsync(s => s.ServiceName.ToLower() == serviceType.ServiceName.ToLower());

            if (exists)
            {
                return BadRequest(new { message = $"Service '{serviceType.ServiceName}' already exists." });
            }

            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceType), new { id = serviceType.ServiceTypeId }, serviceType);
        }

        // PUT: api/servicetypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceType(int id, [FromBody] ServicesTypesModel serviceType)
        {
            if (id != serviceType.ServiceTypeId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingService = await _context.ServiceTypes.FindAsync(id);
            if (existingService == null)
            {
                return NotFound(new { message = $"Service Type with ID {id} not found." });
            }

            // Check if new name conflicts with another service
            var nameConflict = await _context.ServiceTypes
                .AnyAsync(s => s.ServiceName.ToLower() == serviceType.ServiceName.ToLower()
                               && s.ServiceTypeId != id);

            if (nameConflict)
            {
                return BadRequest(new { message = $"Service '{serviceType.ServiceName}' already exists." });
            }

            // Update property
            existingService.ServiceName = serviceType.ServiceName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceTypeExists(id))
                {
                    return NotFound(new { message = $"Service Type with ID {id} not found." });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/servicetypes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteServiceType(int id)
        {
            var serviceType = await _context.ServiceTypes.FindAsync(id);
            if (serviceType == null)
            {
                return NotFound(new { message = $"Service Type with ID {id} not found." });
            }

            // Check if service is being used by any vehicle before deleting
            var isUsed = await _context.VehicleServices.AnyAsync(vs => vs.ServiceTypeId == id);
            if (isUsed)
            {
                return BadRequest(new { message = "Cannot delete service type because it is assigned to one or more vehicles." });
            }

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceTypeExists(int id)
        {
            return _context.ServiceTypes.Any(e => e.ServiceTypeId == id);
        }
    }
}