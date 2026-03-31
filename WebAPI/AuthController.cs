using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data;
using WebAPI.Models;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
       
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized("Nieprawidłowy login lub hasło"); // Poprawiamy komunikat
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, user.Name+" "+user.Surname),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
           
        }),
            Expires = DateTime.UtcNow.AddYears(1), 
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
    
        return Ok(new { Token = tokenString });
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return BadRequest("Użytkownik o tej nazwie już istnieje.");
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                Name = request.Name,
                Surname = request.Surname,
                Username = request.Username,
                PasswordHash = hashedPassword,
                Parish = request.Parish,
                Role = 3
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            var consent = new UserConsent
            {
                UserId = newUser.Id, 
                ConsentType = "terms_and_privacy",
                Status = "accepted",
                DocumentVersion = "v1.0_2026-03",
                IpAddress = request.UserIp ?? "unknown",
                CreatedAt = DateTime.UtcNow
            };
            _context.UserConsents.Add(consent);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { message = "Konto zostało utworzone pomyślnie!" });
        }catch (Exception ex)
        {
            
            await transaction.RollbackAsync();
            return StatusCode(500, $"Błąd: {ex.Message} | Inner: {ex.InnerException?.Message}");
        }
    }

    [HttpGet("CheckSmsPermission")]
    [Authorize]
    public async Task<IActionResult> CheckSmsPermission()
    {

        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new
        {
            CanSend = user.Role <= 2 && user.canSendSMS
        });
    }

    private bool VerifyPassword(string password, string hash)
    {
       return BCrypt.Net.BCrypt.Verify(password, hash);
 
    }
}