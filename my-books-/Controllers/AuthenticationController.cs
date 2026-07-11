using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using my_books.Data;
using my_books.Data.Models;
using my_books.Data.ViewModels;
using my_books.Data.ViewModels.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace my_books.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userExists = await _userManager.FindByEmailAsync(registerVM.Email);
            if (userExists != null)
            {
                return BadRequest("User already exists");
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                Email = registerVM.Email,
                UserName = registerVM.Username,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (!result.Succeeded)
            {
                return BadRequest("User creation failed");
            }

            switch (registerVM.Role)
            {
                case "Admin":
                    await _userManager.AddToRoleAsync(newUser, UserRoles.Admin);
                    break;
                case "Publisher":
                    await _userManager.AddToRoleAsync(newUser, UserRoles.Publisher);
                    break;
                case "Author":
                    await _userManager.AddToRoleAsync(newUser, UserRoles.Author);
                    break;
                default:
                    await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                    break;
            }

            return Created(nameof(Register), $"User {registerVM.Email} created");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, loginVM.Password))
            {
                var tokenResult = await GenerateJwtToken(user);
                return Ok(tokenResult);
            }

            return Unauthorized(new AuthResultVM()
            {
                Success = false
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequestVM tokenRequestVM)
        {
            var result = await VerifyToken(tokenRequestVM);

            if (result == null)
            {
                return BadRequest("Invalid tokens");
            }

            return Ok(result);
        }

        private async Task<AuthResultVM> VerifyToken(TokenRequestVM tokenRequestVM)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParams = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["JWT:Audience"],
                    ValidateLifetime = false // we WANT to validate an expired token here
                };

                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequestVM.Token, tokenValidationParams, out var validatedToken);

                // Check the token's algorithm matches
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                        return null;
                }

                // Check the access token hasn't actually expired yet in a way that's too old
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expiryDate > DateTime.UtcNow)
                {
                    return null; // token hasn't expired yet, refresh not needed/allowed
                }

                // Find the stored refresh token
                var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestVM.RefreshToken);
                if (storedRefreshToken == null)
                    return null;

                if (DateTime.UtcNow > storedRefreshToken.DateExpire)
                    return null;

                if (storedRefreshToken.IsUsed)
                    return null;

                if (storedRefreshToken.IsRevoked)
                    return null;

                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedRefreshToken.JwtId != jti)
                    return null;

                // Mark it used
                storedRefreshToken.IsUsed = true;
                _context.RefreshTokens.Update(storedRefreshToken);
                await _context.SaveChangesAsync();

                // Generate a new token
                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeVal = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeVal = dateTimeVal.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dateTimeVal;
        }


        private async Task<AuthResultVM> GenerateJwtToken(ApplicationUser user)
        {
            var authClaims = new List<Claim>
            {   new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(5),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                DateAdded = DateTime.UtcNow,
                DateExpire = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResultVM()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken.Token,
                Success = true
            };
        }
    }

}