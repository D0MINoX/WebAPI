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
        public async Task<IActionResult> GetMeditationByDateAndTitle([FromQuery] int? date, [FromQuery] string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Tytuł (tajemnica) jest wymagany.");
            }
            var query = _context.Meditations
        .Where(m => m.Title.ToLower() == title.ToLower());
          
            if (date.HasValue && date > 0)
            {
                var meditation = await query
                    .Select(m => new {
                        m.Date, 
                        m.Content,
                        m.Link
                    })
                    .FirstOrDefaultAsync();

                if (meditation == null) return NotFound("Brak rozważania na wybrany dzień.");
                return Ok(new List<object> { meditation });
            }
            var allMeditations = await query
                .OrderBy(m => m.Date)
                .Select(m => new {
                    m.Date,
                    m.Content,
                    m.Link
                })
                .ToListAsync();

            return Ok(allMeditations);
        }
    }
}
