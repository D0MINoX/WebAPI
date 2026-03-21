using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI
{
 
        [ApiController]
        [Route("api/[controller]")]
        public class RosariesController : ControllerBase
        {
            private readonly ApplicationDbContext _context;
            private readonly IConfiguration _configuration;
            public RosariesController(ApplicationDbContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }
            [HttpGet("user/{userId}/rosaries")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRosaryNames(int userId)
        {
           
            var names = await _context.UsersRosary
                .Include(ur => ur.Rosary)
                .Where(ur => ur.UserId == userId && ur.isAuthorized)
                .Select(ur => new
                {
                    Id = ur.RosaryId,
                    Name = ur.Rosary.Name
                }) 
                .ToListAsync();

            if (names == null || !names.Any())
            {
                return NotFound(); 
            }

            return Ok(names);
        }
        [HttpGet("rosaries")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRosaryNames()
        {

            var names = await _context.Rosary
                
                .Select(ur => new
                {
                    Id = ur.Id,
                    Name = ur.Name
                })
                .ToListAsync();

            if (names == null || !names.Any())
            {
                return NotFound();
            }

            return Ok(names);
        }

        [HttpPost("JoinRosary")]
        public async Task<ActionResult<IEnumerable<bool>>> JoinRosary([FromBody] RosaryJoinRequest request)
        {
            if (await _context.UsersRosary.AnyAsync(u => u.RosaryId == request.RosaryId && u.UserId == request.UserId))
                return BadRequest("użytkownik już został wpisany do tej róży");
            var newRosaryConn = new UsersRosary
            {
                UserId = request.UserId,
                RosaryId = request.RosaryId
            };
            _context.UsersRosary.Add(newRosaryConn);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Dodano użytkownika do róży" });
        }

        [HttpGet("rosary/{RosaryId}/Name")]
        public async Task<IActionResult> RosaryName(int RosaryId) {
            var names = await _context.Rosary
                .Where(ur => ur.Id == RosaryId)
                .Select(ur => new
                {
                    Name = ur.Name
                }).FirstOrDefaultAsync();
            
            if (names == null)
            {
                return NotFound();
            }

            return Ok(names);
        }
    }
}
   
