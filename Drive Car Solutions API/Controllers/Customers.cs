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
    public class Customers : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthorizationService _authorizationService;

        public Customers(AppDbContext context, IAuthorizationService authorizationService)
        {
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomersModel>>> GetCustomers()
        {
            var query = _context.Customers
                .Include(c => c.Vehicles)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(c => c.ApplicationUserId == userId);
            }

            return await query.ToListAsync();
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomersModel>> GetCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            if (!User.IsInRole("Admin") && customer.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return Forbid();
            }

            return customer;
        }

        // POST: api/customers
        [HttpPost]
        public async Task<ActionResult<CustomersModel>> CreateCustomer([FromBody] CustomersModel customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            customer.ApplicationUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.CustomerId }, customer);
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomersModel customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, existingCustomer, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            // Update properties
            existingCustomer.FullName = customer.FullName;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound(new { message = $"Customer with ID {id} not found." });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = $"Customer with ID {id} not found." });
            }

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, customer, new SameOwnerRequirement());
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}