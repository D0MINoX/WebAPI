using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Data;
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
        private readonly string _connectionString;
        public MeditationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> GetMeditationByDateAndTitle([FromQuery] int? date, [FromQuery] string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Tytuł (tajemnica) jest wymagany.");
            }
            var query = _context.Meditations
        .Where(m => m.Title.ToLower() == title.ToLower());
          
            if (date.HasValue)
            {
                var meditation = await query
                    .Select(m => new {
                        m.Date, 
                        m.Content,
                        m.Link
                    }).Where(m=>m.Date==date)
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
        [AllowAnonymous]
        [HttpPost("RecordPrayer")]
        public async Task<IActionResult> RecordPrayer([FromBody] Record request)
        {
            if (request == null || request.UserId <= 0)
            {
                return BadRequest(new { message = "Nieprawidłowe dane użytkownika." });
            }

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new MySqlCommand("RecordPrayer", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("p_UserId", request.UserId);
                        command.Parameters.AddWithValue("p_Date", request.Date.Date);

                        await command.ExecuteNonQueryAsync();
                    }
                }

                return Ok(new { message = "Modlitwa została zarejestrowana." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Błąd bazy danych.", details = ex.Message });
            }
        }
    }
}
