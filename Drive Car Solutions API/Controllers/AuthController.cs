using Drive_Car_Solutions_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Drive_Car_Solutions_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly Drive_Car_Solutions_API.Data.AppDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, Drive_Car_Solutions_API.Data.AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = $"User creation failed! Errors: {errors}" });
            }

            // Assign the 'User' role by default
            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost("register-by-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterByAdmin([FromBody] RegisterByAdminModel model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            var normalizedRole = model.Role.Trim();
            if (!normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && 
                !normalizedRole.Equals("Staff", StringComparison.OrdinalIgnoreCase) &&
                !normalizedRole.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Status = "Error", Message = "Role must be 'Admin', 'Staff', or 'User'!" });
            }

            string finalRole = "User";
            if (normalizedRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                finalRole = "Admin";
            }
            else if (normalizedRole.Equals("Staff", StringComparison.OrdinalIgnoreCase))
            {
                finalRole = "Staff";
            }

            ApplicationUser user = new ApplicationUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = $"User creation failed! Errors: {errors}" });
            }

            if (!await _roleManager.RoleExistsAsync(finalRole))
                await _roleManager.CreateAsync(new IdentityRole(finalRole));

            await _userManager.AddToRoleAsync(user, finalRole);

            return Ok(new { Status = "Success", Message = $"{finalRole} user created successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var keySection = _configuration["Jwt:Key"] ?? "your_super_secret_key_that_is_at_least_32_characters_long";
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySection));

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddMinutes(15),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                var refreshToken = GenerateRefreshToken();
                var refreshTokenEntity = new RefreshToken
                {
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    ApplicationUserId = user.Id
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    refreshToken = refreshToken,
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.RefreshToken))
            {
                return BadRequest(new { Status = "Error", Message = "Refresh token is required." });
            }

            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.ApplicationUser)
                .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow)
            {
                return Unauthorized(new { Status = "Error", Message = "Invalid or expired refresh token." });
            }

            var user = refreshToken.ApplicationUser;
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var keySection = _configuration["Jwt:Key"] ?? "your_super_secret_key_that_is_at_least_32_characters_long";
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keySection));

            var newAccessToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddMinutes(15),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var newRefreshToken = GenerateRefreshToken();
            refreshToken.IsRevoked = true;

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                ApplicationUserId = user.Id
            };

            _context.RefreshTokens.Update(refreshToken);
            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
                expiration = newAccessToken.ValidTo
            });
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.RefreshToken))
            {
                return BadRequest(new { Status = "Error", Message = "Refresh token is required." });
            }

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken);

            if (refreshToken == null)
            {
                return NotFound(new { Status = "Error", Message = "Refresh token not found." });
            }

            refreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = "Refresh token revoked successfully." });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
