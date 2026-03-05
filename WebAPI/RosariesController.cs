using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

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
                .Where(ur => ur.UserId == userId)
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
    }
}
