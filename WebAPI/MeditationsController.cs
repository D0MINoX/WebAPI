using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using WebAPI.Data;
using WebAPI.Models;
namespace WebAPI
{
    [ApiController] 
    [Route("api/[controller]")]
    public class MeditationsController:ControllerBase
    {
         private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public MeditationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetMeditationByDateAndTitle([FromQuery] DateTime date, [FromQuery] string title)
        {
         
            var meditation = await _context.Meditations
                .Where(m => m.Date.Date == date.Date && m.Title.ToLower() == title.ToLower())
                .Select(m=>m.Content)
                .FirstOrDefaultAsync();

            if (meditation == null)
            {
                return NotFound("Brak rozważania na wybrany dzień.");
            }

            return Ok(meditation);
        }
    }
}
