using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;
using System.Globalization;

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
                .Include(m => m.VenueMappings)
                    .ThenInclude(v => v.Venue)
                .Include(m => m.UserMappings)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            return meetings.Select(m => m.ToSummaryDto()).ToList();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<MeetingStatsDto>> GetStats([FromQuery] string? period = "month")
        {
            var meetings = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.VenueMappings)
                    .ThenInclude(v => v.Venue)
                .Include(m => m.UserMappings)
                    .ThenInclude(u => u.User)
                .OrderBy(m => m.Date)
                .ToListAsync();

            var normalizedPeriod = NormalizePeriod(period);

            return new MeetingStatsDto
            {
                TotalMeetings = meetings.Count,
                TotalPresentAttendees = meetings.Sum(m => m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && u.IsPresent)),
                TotalAbsentAttendees = meetings.Sum(m => m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && !u.IsPresent)),
                MeetingVolume = BuildMeetingVolume(meetings, normalizedPeriod),
                MeetingAttendance = meetings
                    .Select(m => new MeetingAttendancePointDto
                    {
                        MeetingId = m.Id,
                        Label = !string.IsNullOrWhiteSpace(m.MeetingNumber) ? m.MeetingNumber! : m.Date.ToString("dd MMM"),
                        PresentCount = m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && u.IsPresent),
                        AbsentCount = m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && !u.IsPresent)
                    })
                    .ToList(),
                UserAttendance = meetings
                    .SelectMany(m => m.UserMappings)
                    .Where(u => u.Role == MeetingRoles.Attendee)
                    .GroupBy(u => new { u.UserId, u.User.UserName })
                    .Select(g => new UserAttendancePointDto
                    {
                        UserId = g.Key.UserId,
                        UserName = g.Key.UserName,
                        PresentCount = g.Count(x => x.IsPresent),
                        AbsentCount = g.Count(x => !x.IsPresent)
                    })
                    .OrderByDescending(x => x.TotalCount)
                    .ThenBy(x => x.UserName)
                    .ToList(),
                VenueMeetings = meetings
                    .SelectMany(m => m.VenueMappings.Select(v => new
                    {
                        v.VenueId,
                        v.Venue.VenueName,
                        PresentCount = m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && u.IsPresent),
                        AbsentCount = m.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && !u.IsPresent)
                    }))
                    .GroupBy(v => new { v.VenueId, v.VenueName })
                    .Select(g => new VenueMeetingPointDto
                    {
                        VenueId = g.Key.VenueId,
                        VenueName = g.Key.VenueName,
                        MeetingCount = g.Count(),
                        AveragePresentCount = Math.Round(g.Average(x => x.PresentCount), 1),
                        AverageAbsentCount = Math.Round(g.Average(x => x.AbsentCount), 1)
                    })
                    .OrderByDescending(x => x.MeetingCount)
                    .ThenBy(x => x.VenueName)
                    .ToList()
            };
        }

        private static string NormalizePeriod(string? period)
        {
            return period?.Trim().ToLowerInvariant() switch
            {
                "week" => "week",
                "year" => "year",
                _ => "month"
            };
        }

        private static List<MeetingVolumePointDto> BuildMeetingVolume(List<Meeting> meetings, string period)
        {
            return period switch
            {
                "week" => meetings
                    .GroupBy(m => new
                    {
                        Year = ISOWeek.GetYear(m.Date),
                        Week = ISOWeek.GetWeekOfYear(m.Date)
                    })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Week)
                    .Select(g => new MeetingVolumePointDto
                    {
                        Label = $"W{g.Key.Week:D2} {g.Key.Year}",
                        Count = g.Count()
                    })
                    .ToList(),
                "year" => meetings
                    .GroupBy(m => m.Date.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => new MeetingVolumePointDto
                    {
                        Label = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList(),
                _ => meetings
                    .GroupBy(m => new { m.Date.Year, m.Date.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new MeetingVolumePointDto
                    {
                        Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                        Count = g.Count()
                    })
                    .ToList()
            };
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MeetingDto>> GetMeeting(int id)
        {
            var meeting = await _context.Meetings
                .AsNoTracking()
                .Include(m => m.VenueMappings)
                    .ThenInclude(v => v.Venue)
                .Include(m => m.UserMappings)
                    .ThenInclude(u => u.User)
                .Include(m => m.Agendas)
                    .ThenInclude(a => a.OwnerUser)
                .Include(m => m.ActionItems)
                    .ThenInclude(a => a.ResponsibilityUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting is null)
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

            var entity = await meeting.ToEntityAsync(_context);
            _context.Meetings.Add(entity);
            await _context.SaveChangesAsync();

            var created = await LoadMeetingGraphAsync(entity.Id);
            return CreatedAtAction(nameof(GetMeeting), new { id = entity.Id }, created!.ToDto());
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutMeeting(int id, MeetingUpsertDto meeting)
        {
            if (!ValidateMeeting(meeting))
            {
                return ValidationProblem(ModelState);
            }

            var existingMeeting = await _context.Meetings
                .Include(m => m.VenueMappings)
                .Include(m => m.UserMappings)
                    .ThenInclude(u => u.User)
                .Include(m => m.Agendas)
                    .ThenInclude(a => a.OwnerUser)
                .Include(m => m.ActionItems)
                    .ThenInclude(a => a.ResponsibilityUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMeeting is null)
            {
                return NotFound();
            }

            await meeting.ApplyToEntityAsync(existingMeeting, _context);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteMeeting(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting is null)
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

            if ((meeting.Facilitator?.Id ?? 0) < 0 ||
                (meeting.Chairperson?.Id ?? 0) < 0 ||
                (meeting.Secretary?.Id ?? 0) < 0 ||
                (meeting.Venue?.Id ?? 0) < 0 ||
                meeting.Attendees.Any(a => a.MappingId < 0 || (a.UserId ?? 0) < 0) ||
                meeting.Agendas.Any(a => a.Id < 0 || (a.Owner?.Id ?? 0) < 0) ||
                meeting.ActionItems.Any(a => a.Id < 0 || (a.Responsibility?.Id ?? 0) < 0))
            {
                ModelState.AddModelError(nameof(meeting.Attendees), "Ids must be zero or greater.");
            }

            return ModelState.IsValid;
        }

        private Task<Meeting?> LoadMeetingGraphAsync(int id)
        {
            return _context.Meetings
                .AsNoTracking()
                .Include(m => m.VenueMappings)
                    .ThenInclude(v => v.Venue)
                .Include(m => m.UserMappings)
                    .ThenInclude(u => u.User)
                .Include(m => m.Agendas)
                    .ThenInclude(a => a.OwnerUser)
                .Include(m => m.ActionItems)
                    .ThenInclude(a => a.ResponsibilityUser)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
