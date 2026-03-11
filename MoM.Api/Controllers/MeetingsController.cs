using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;

namespace MoM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingsController : ControllerBase
    {
        private readonly MomContext _context;

        public MeetingsController(MomContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeetingSummaryDto>>> GetMeetings()
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.MeetingUsers)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            return meetings.Select(m => m.ToSummaryDto()).ToList();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<MeetingStatsDto>> GetStats()
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.MeetingUsers)
                .OrderBy(m => m.Date)
                .ToListAsync();

            var stats = new MeetingStatsDto
            {
                TotalMeetings = meetings.Count,
                TotalPresentAttendees = meetings.Sum(m => m.MeetingUsers.Count(u => u.IsPresent)),
                TotalAbsentAttendees = meetings.Sum(m => m.MeetingUsers.Count(u => !u.IsPresent)),
                MeetingVolume = meetings
                    .GroupBy(m => new { m.Date.Year, m.Date.Month })
                    .Select(g => new MeetingVolumePointDto
                    {
                        Label = $"{g.Key.Month:D2}/{g.Key.Year}",
                        Count = g.Count()
                    })
                    .ToList(),
                MeetingAttendance = meetings
                    .Select(m => new MeetingAttendancePointDto
                    {
                        MeetingId = m.Id,
                        Label = !string.IsNullOrWhiteSpace(m.MeetingNumber)
                            ? m.MeetingNumber!
                            : m.Date.ToString("dd MMM"),
                        PresentCount = m.MeetingUsers.Count(u => u.IsPresent),
                        AbsentCount = m.MeetingUsers.Count(u => !u.IsPresent)
                    })
                    .ToList()
            };

            return stats;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MeetingDto>> GetMeeting(int id)
        {
            var meeting = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.MeetingUsers)
                .Include(m => m.Agendas)
                .Include(m => m.ActionItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return NotFound();
            }

            return meeting.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<MeetingDto>> PostMeeting(MeetingUpsertDto meeting)
        {
            if (!ValidateMeeting(meeting))
            {
                return ValidationProblem(ModelState);
            }

            var entity = meeting.ToEntity();
            _context.Meetings.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeeting), new { id = entity.Id }, entity.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMeeting(int id, MeetingUpsertDto meeting)
        {
            if (!ValidateMeeting(meeting))
            {
                return ValidationProblem(ModelState);
            }

            var existingMeeting = await _context.Meetings
                .Include(m => m.MeetingUsers)
                .Include(m => m.Agendas)
                .Include(m => m.ActionItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMeeting == null)
            {
                return NotFound();
            }

            meeting.ApplyToEntity(existingMeeting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound();
            }

            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ValidateMeeting(MeetingUpsertDto meeting)
        {
            if (meeting.Date is null)
            {
                ModelState.AddModelError(nameof(meeting.Date), "Date is required.");
            }

            if (meeting.MeetingUsers.Any(u => u.Id < 0) ||
                meeting.Agendas.Any(a => a.Id < 0) ||
                meeting.ActionItems.Any(a => a.Id < 0))
            {
                ModelState.AddModelError(nameof(meeting.MeetingUsers), "Child item ids must be zero or greater.");
            }

            return ModelState.IsValid;
        }
    }
}
