using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParishController:ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ParishController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpGet("getAllParish")]
        public async Task<IActionResult> GetParish()
        {
            var parish = await _context.Parishes.Select(ur => new
            {
                Id = ur.Id,
                Name = ur.Name
            }).ToListAsync();
            
            if(parish==null || !parish.Any())
            {
                return NotFound("Nie znaleziono parafii");
            }
            return Ok(parish);
        }
        [AllowAnonymous]
        [HttpGet("getUserParish/{UserId}")]
        public async Task<IActionResult> GetUserParish(int UserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            if (user == null)
            {
                return NotFound("Nie znaleziono użytkownika");
            }
            if (user.Parish == null)
            {
                return Ok(new { Id = -1, Name = "Brak przypisanej parafii" });
            }
            var parish = await _context.Parishes
                .Where(p=>p.Id==user.Parish)
                .Select(ur => new
            {
                Id = ur.Id,
                Name = ur.Name
            }).FirstOrDefaultAsync();

            if (parish == null)
            {
                return Ok(new { Id = -1, Name = "Brak przypisanej parafii" });
            }
            return Ok(parish);
        }
    }
}
