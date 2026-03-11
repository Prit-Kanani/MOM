using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;

namespace MoM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupsController : ControllerBase
    {
        private readonly MomContext _context;

        public LookupsController(MomContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<LookupItemDto>>> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .Select(u => new LookupItemDto
                {
                    Id = u.Id,
                    Name = u.UserName
                })
                .ToListAsync();

            return users;
        }

        [HttpGet("venues")]
        public async Task<ActionResult<IEnumerable<LookupItemDto>>> GetVenues()
        {
            var venues = await _context.Venues
                .AsNoTracking()
                .OrderBy(v => v.VenueName)
                .Select(v => new LookupItemDto
                {
                    Id = v.Id,
                    Name = v.VenueName
                })
                .ToListAsync();

            return venues;
        }
    }
}
