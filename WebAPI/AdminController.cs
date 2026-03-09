using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpGet("{rosaryId}/usersShow")]
        public async Task<IActionResult> UserShow(int rosaryId)
        {
            var names = await _context.UsersRosary
                .Where(ur => ur.RosaryId == rosaryId && ur.User.Role > 2)
                .Select(ur => new
                {
                    UserId = ur.UserId,
                    UserName = ur.User.Name,
                    UserSurname = ur.User.Surname,
                    RosaryName = ur.Rosary.Name,
                    IsAuthorized = ur.isAuthorized
                })
                .ToListAsync();

            if (names == null || !names.Any())
            {
                return NotFound("API: nie znaleziono");
            }

            return Ok(names);
        }
        [HttpPut("{userId}/Authorization/{rosaryId}")]
        public async Task<IActionResult> UserAuthorization(int userId,int rosaryId)
        {
            var membership = await _context.UsersRosary.FirstOrDefaultAsync(ur=>ur.UserId==userId && ur.RosaryId==rosaryId);

            if (membership == null)
                return NotFound("Nie znaleziono takiego powiązania.");

            
            membership.isAuthorized = true;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Użytkownik został pomyślnie zweryfikowany!" });
        }
        [HttpDelete("delete-membership/{userId}/{rosaryId}")]
        public async Task<IActionResult> DeleteMembership(int userId, int rosaryId)
        {
           
            var membership = await _context.UsersRosary
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RosaryId == rosaryId);

            if (membership == null)
                return NotFound("Nie znaleziono takiego powiązania.");

           
            _context.UsersRosary.Remove(membership);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Użytkownik został usunięty z róży." });
        }
    }
}
