using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
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
        [Authorize(Roles = "0,1,2")]
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
        [Authorize(Roles = "0,1")]
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
        [Authorize(Roles = "0,1,2")]
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
        [Authorize(Roles = "0,1,2")]
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
        [Authorize(Roles = "0,1")]
        [HttpPost("AddRosary")]
        public async Task<IActionResult> AddRosary([FromBody] RosaryAddRrquest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                var newRosary = new Rosary
                {
                    Name = request.Name,
                    ParishValue = request.Parish
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
        [Authorize(Roles = "0,1,2")]
        [HttpPut("ModifyMeditation")]
        public async Task<IActionResult> ModifyMeditation([FromBody] Meditation request) {
            var Meditation = await _context.Meditations.FirstOrDefaultAsync(ur => ur.Title == request.Title && ur.Date==request.Date);
            if (Meditation == null)
            {
                return NotFound("Błąd bazy danych");
            }
            Meditation.Content = request.Content;
            Meditation.Link = request.Link;
            await _context.SaveChangesAsync();
            return Ok(new { message = "zmieniono treść rozważania" });
        }
        [Authorize(Roles = "0,1,2")]
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
                    UserRole = ur.Role,
                    UserCanSendSMS= ur.canSendSMS
                    
                })
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound("API: nie znaleziono");
            }

            return Ok(users);
        }

        /*   [HttpPut("UpdateRole")]
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
           }*/
        [Authorize(Roles = "0,1")]
        [HttpGet("MainZelatorsShow")]
        public async Task<IActionResult> MainZelatorsShow()
        {
            var names = await _context.Users
                .Where(ur => ur.Role == 1&&ur.Parish==null)
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
        [Authorize(Roles = "0,1")]
        [HttpPost("AddParish")]
        public async Task<IActionResult> AddParish([FromBody] ParishAddRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newParish = new Parish
                {
                    Name = request.Name,
             
                };
                _context.Parishes.Add(newParish);
                await _context.SaveChangesAsync();
                var zelator = await _context.Users.FindAsync(request.ZelatorsId);

                if (zelator == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Nie znaleziono użytkownika o podanym Id Zelatora.");
                }

              
                zelator.Parish = newParish.Id;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { message = "Utworzono różę" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Błąd: {ex.Message}");
            }
        }
        [Authorize(Roles = "0,1,2")]
        [HttpPut("UpdatePermissions")]
        public async Task<IActionResult> UpdatePermissions([FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(ur => ur.Id == request.Id);
            if (user == null) return NotFound();

            user.Role = request.Role;
            user.canSendSMS = request.CanSendSMS;
            user.TokenVersion++;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [Authorize(Roles ="0")]
        [HttpGet("ConsentsShow")]
        public async Task<IActionResult> ConsentsShow()
        {
            var names = await _context.UserConsents
                .Select(uc => new
                {
                    UserId = uc.UserId,
                    ConsentType = uc.ConsentType,
                    Status = uc.Status,
                    DocumentVersion = uc.DocumentVersion,
                    IpAddress = uc.IpAddress,
                    ExternalMemberId=uc.ExternalMemberId,
                    CreatedAt = uc.CreatedAt
                })
                .ToListAsync();

            if (names == null || !names.Any())
            {
                return NotFound("API: nie znaleziono");
            }

            return Ok(names);
        }
        [Authorize(Roles = "0,1,2")]
        [HttpDelete("deleteExternalNumber/{userId}/{rosaryId}")]
        public async Task<IActionResult> DeleteExternalNumber(int userId, int rosaryId)
        {

            var membership = await _context.ExternalMembers
                .FirstOrDefaultAsync(ur => ur.Id == userId && ur.RosaryId == rosaryId);

            if (membership == null)
                return NotFound("Nie znaleziono takiego powiązania.");


            _context.ExternalMembers.Remove(membership);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Użytkownik został usunięty z róży." });
        }
        [Authorize(Roles = "0,1,2")]
        [HttpPost("AddExternalMember")]
        public async Task<IActionResult> AddExternalMember([FromBody] ExternalMemberRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var newExMember = new ExternalMember
                {
                    Name = request.FirstName,
                    Surname = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    RosaryId = request.RosaryId,
                    CreatedAt = DateTime.Now

                };
                _context.ExternalMembers.Add(newExMember);
                await _context.SaveChangesAsync();

                var consent = new UserConsent
                {
                    ExternalMemberId = newExMember.Id,
                    ConsentType = "sms_contact_consent",
                    Status = "accepted",
                    DocumentVersion = "v1.0_2026-03",
                    IpAddress = request.UserIp ?? "unknown",
                    CreatedAt = DateTime.UtcNow
                };
                _context.UserConsents.Add(consent);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Konto zostało utworzone pomyślnie!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Błąd: {ex.ToString()}");
            }
        }
        [Authorize(Roles = "0,1,2")]
        [HttpPut("UpdateExternalMember")]
        public async Task<IActionResult> UpdateExternalMember([FromBody] UpdateExternalMemberRequest request)
        {
            var user= await _context.ExternalMembers.FirstOrDefaultAsync(ur => ur.Id == request.Id);
            if (user == null) return NotFound();
            user.Name = request.Name;
            user.Surname = request.Surname;
            user.PhoneNumber = request.PhoneNumber;
            await _context.SaveChangesAsync();
            return Ok();
        }
        /* do dopracowania usuwać można tylko mniejsze role*/
        [Authorize(Roles = "0,1,2")]
        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user == null)
            {
                return Unauthorized("Nieznaleziono");
            }
            else
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Ok(new { message = "Konto zostało usunięte" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Błąd: {ex.Message} | Inner: {ex.InnerException?.Message}");
                }
            }
        }
    }
}
    
