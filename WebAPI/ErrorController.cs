using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ErrorController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Zgłaszanie błędu przez użytkownika
        [AllowAnonymous]
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitError([FromBody] ErrorReport errorReport)
        {
            if (string.IsNullOrWhiteSpace(errorReport.ErrorMessage))
            {
                return BadRequest("Opis błędu jest wymagany.");
            }

            errorReport.CreatedAt = DateTime.UtcNow; // Wypełnienie daty utworzenia
            errorReport.Status = "Nowe";            // Domyślny status zgłoszenia

            // Id nie trzeba ustawiać, bo generuje je baza (AUTO_INCREMENT)
            await _context.ErrorReports.AddAsync(errorReport);
            await _context.SaveChangesAsync();

            return Ok("Twoje zgłoszenie zostało przyjęte.");
        }

        // Pobieranie listy wszystkich zgłoszeń błędów
        [HttpGet("all")]
        public async Task<IActionResult> GetAllErrors()
        {
            var errors = await _context.ErrorReports
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.UserPhone,
                    e.ErrorMessage,
                    e.Status,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(errors);
        }

        // Pobieranie szczegółów zgłoszenia po ID
        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetErrorDetails(int id)
        {
            var error = await _context.ErrorReports
                .Where(e => e.Id == id)
                .Select(e => new
                {
                    e.Id,
                    e.UserPhone,
                    e.ErrorMessage,
                    e.Status,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (error == null)
            {
                return NotFound("Nie znaleziono zgłoszenia o podanym ID.");
            }

            return Ok(error);
        }

        // Aktualizacja statusu zgłoszenia błędu
        [HttpPut("update-status/{id}")]
        public async Task<IActionResult> UpdateErrorStatus(int id, [FromQuery] string status)
        {
            // Walidacja statusu
            if (string.IsNullOrWhiteSpace(status) || !new[] { "Nowe", "Odebrane", "Zamknięte" }.Contains(status))
            {
                return BadRequest("Nieprawidłowy status. Prawidłowe wartości to: 'Nowe', 'Odebrane', 'Zamknięte'.");
            }

            var error = await _context.ErrorReports.FindAsync(id);
            if (error == null)
            {
                return NotFound("Nie znaleziono zgłoszenia o podanym ID.");
            }

            error.Status = status;
            error.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok($"Status zgłoszenia o ID {id} został zaktualizowany do '{status}'.");
        }
    }
}