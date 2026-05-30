using Drive_Car_Solutions_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drive_Car_Solutions_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]// All endpoints in this controller require Admin role
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    Roles = roles
                });
            }

            return Ok(result);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Status = "Error", Message = $"User with ID '{id}' not found." });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles
            });
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Status = "Error", Message = $"User with ID '{id}' not found." });

            // Validate the requested role
            var normalizedRole = model.Role.Trim();
            if (!normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) &&
                !normalizedRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) &&
                !normalizedRole.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Status = "Error", Message = "Role must be 'Admin', 'Staff', or 'User'." });
            }

            // Check if new username or email conflicts with another account
            var existingByName = await _userManager.FindByNameAsync(model.Username);
            if (existingByName != null && existingByName.Id != id)
                return BadRequest(new { Status = "Error", Message = "Username is already taken by another user." });

            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail != null && existingByEmail.Id != id)
                return BadRequest(new { Status = "Error", Message = "Email is already in use by another user." });

            // Apply username / email changes
            user.UserName = model.Username;
            user.Email = model.Email;
            user.NormalizedUserName = model.Username.ToUpper();
            user.NormalizedEmail = model.Email.ToUpper();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return StatusCode(500, new { Status = "Error", Message = $"Failed to update user: {errors}" });
            }

            // Optionally update password
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    var errors = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
                    return StatusCode(500, new { Status = "Error", Message = $"Failed to update password: {errors}" });
                }
            }

            // Update role: remove existing roles then assign the new one
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            string finalRole = normalizedRole.Substring(0, 1).ToUpper() + normalizedRole.Substring(1).ToLower();
            if (!await _roleManager.RoleExistsAsync(finalRole))
                await _roleManager.CreateAsync(new IdentityRole(finalRole));

            await _userManager.AddToRoleAsync(user, finalRole);

            return Ok(new { Status = "Success", Message = "User updated successfully." });
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Status = "Error", Message = $"User with ID '{id}' not found." });

            var deleteResult = await _userManager.DeleteAsync(user);
            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
                return StatusCode(500, new { Status = "Error", Message = $"Failed to delete user: {errors}" });
            }

            return Ok(new { Status = "Success", Message = "User deleted successfully." });
        }
    }
}
