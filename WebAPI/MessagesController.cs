using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
namespace WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController:ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public MessagesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpGet("getMessages/{rosaryId}")]
        public async Task<IActionResult> GetMessages(int rosaryId)
        {
          
            var messages = await _context.RosaryMessages.Where(ur => ur.RosaryId == rosaryId).Select(ur => ur).ToListAsync();
            if (messages.Any())
            {

                return Ok(messages);
                
            }
            else
            {
                return NotFound("API: nie znaleziono");
            }

        }
        [HttpPost("newMessage")]
        public async Task<IActionResult> newMessage([FromBody] RosaryMessages message)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                _context.RosaryMessages.Add(message);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok();
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Błąd: {ex.Message}");
            }
        }
    }
}
