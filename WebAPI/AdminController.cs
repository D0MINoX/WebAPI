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
        [HttpGet("zelatorsShow")]
        public async Task<IActionResult> ZelatorsShow()
        {
            var names = await _context.Users
                .Where(ur => ur.Role == 2)
                .Select(ur => new
                {
                    UserId = ur.Id,
                    UserName = ur.Name,
                    UserSurname = ur.Surname,
                })
                .ToListAsync();

            if (names == null || !names.Any())
            {
                return NotFound("API: nie znaleziono");
            }

            return Ok(names);
        }
        [HttpPut("{userId}/Authorization/{rosaryId}")]
        public async Task<IActionResult> UserAuthorization(int userId, int rosaryId)
        {
            var membership = await _context.UsersRosary.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RosaryId == rosaryId);

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

        [HttpPost("AddRosary")]
        public async Task<IActionResult> AddRosary([FromBody] RosaryAddRrquest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                var newRosary = new Rosary
                {
                    Name = request.Name,
                    Parish = request.Parish
                };
                _context.Rosary.Add(newRosary);
                await _context.SaveChangesAsync();
                var userIdsToAdd = await _context.Users
                .Where(u => u.Role == 0 || u.Role == 1 || u.Id == request.ZelatorsId)
                .Select(u => u.Id)
                .Distinct()
                .ToListAsync();
                var links = userIdsToAdd.Select(userId => new UsersRosary
                {
                    RosaryId = newRosary.Id,
                    UserId = userId,
                    isAuthorized = true
                }).ToList();

                _context.UsersRosary.AddRange(links);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Utworzono różę" });
            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Błąd: {ex.Message}");
            }
        }
        [HttpPut("ModifyMeditation")]
        public async Task<IActionResult> ModifyMeditation([FromBody] Meditation request) {
            var Meditation = await _context.Meditations.FirstOrDefaultAsync(ur => ur.Title == request.Title && ur.Date==request.Date);
            if (Meditation == null)
            {
                return NotFound("Błąd bazy danych");
            }
            Meditation.Content = request.Content;
            await _context.SaveChangesAsync();
            return Ok(new { message = "zmieniono treść rozważania" });
        }
        [HttpGet("usersShow/{UserRole}")]
        public async Task<IActionResult> UsersPrivilagiesShow(int UserRole)
        {
            var users = await _context.Users
                .Where(ur => ur.Role>=UserRole)
                .Select(ur => new
                {
                    UserId = ur.Id,
                    UserName = ur.Name,
                    UserSurname = ur.Surname,
                    UserEmail = ur.Username,
                    UserRole = ur.Role
                    
                })
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound("API: nie znaleziono");
            }

            return Ok(users);
        }

        [HttpPut("UpdateRole")]
        public async Task<IActionResult> updateRole ([FromBody]UpdateUserRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(ur => ur.Id==request.Id);
            if (user == null)
            {
                return NotFound("Błąd bazy danych");
            }
            user.Role = request.Role;
            await _context.SaveChangesAsync();
            return Ok(new { message = "zmieniono uprawnienia" });
        }
    }
}
    
