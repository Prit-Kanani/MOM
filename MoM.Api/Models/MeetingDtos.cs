using System.ComponentModel.DataAnnotations;

namespace MoM.Api.Models
{
    public class MeetingSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string? MeetingNumber { get; set; }
        public string? MeetingType { get; set; }
        public DateTime Date { get; set; }
        public string? VenueName { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class MeetingDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? MeetingNumber { get; set; }
        public string? MeetingType { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Time { get; set; }
        public LookupSelectionDto? Venue { get; set; }
        public LookupSelectionDto? Facilitator { get; set; }
        public LookupSelectionDto? Chairperson { get; set; }
        public LookupSelectionDto? Secretary { get; set; }
        public string? Logo { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalAttendees { get; set; }
        public List<MeetingAttendeeDto> Attendees { get; set; } = new();
        public List<AgendaItemDto> Agendas { get; set; } = new();
        public List<ActionItemDto> ActionItems { get; set; } = new();
    }

    public class MeetingUpsertDto
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = "Minutes of Meeting";

        [MaxLength(50)]
        public string? MeetingNumber { get; set; }

        [MaxLength(100)]
        public string? MeetingType { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime? Date { get; set; }

        [MaxLength(50)]
        public string? Time { get; set; }

        public LookupSelectionUpsertDto? Venue { get; set; }
        public LookupSelectionUpsertDto? Facilitator { get; set; }
        public LookupSelectionUpsertDto? Chairperson { get; set; }
        public LookupSelectionUpsertDto? Secretary { get; set; }
        public string? Logo { get; set; }
        public List<MeetingAttendeeUpsertDto> Attendees { get; set; } = new();
        public List<AgendaItemUpsertDto> Agendas { get; set; } = new();
        public List<ActionItemUpsertDto> ActionItems { get; set; } = new();
    }

    public class LookupSelectionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class LookupSelectionUpsertDto
    {
        public int? Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }
    }

    public class MeetingAttendeeDto
    {
        public int MappingId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }

    public class MeetingAttendeeUpsertDto
    {
        public int MappingId { get; set; }
        public int? UserId { get; set; }

        [MaxLength(200)]
        public string? UserName { get; set; }

        public bool IsPresent { get; set; }
    }

    public class AgendaItemDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public LookupSelectionDto? Owner { get; set; }
    }

    public class AgendaItemUpsertDto
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Topic { get; set; } = string.Empty;

        public LookupSelectionUpsertDto? Owner { get; set; }
    }

    public class ActionItemDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string Task { get; set; } = string.Empty;
        public LookupSelectionDto? Responsibility { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class ActionItemUpsertDto
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Task { get; set; } = string.Empty;

        public LookupSelectionUpsertDto? Responsibility { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class LookupItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class MeetingStatsDto
    {
        public int TotalMeetings { get; set; }
        public int TotalPresentAttendees { get; set; }
        public int TotalAbsentAttendees { get; set; }
        public List<MeetingVolumePointDto> MeetingVolume { get; set; } = new();
        public List<MeetingAttendancePointDto> MeetingAttendance { get; set; } = new();
        public List<UserAttendancePointDto> UserAttendance { get; set; } = new();
        public List<VenueMeetingPointDto> VenueMeetings { get; set; } = new();
    }

    public class MeetingVolumePointDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MeetingAttendancePointDto
    {
        public int MeetingId { get; set; }
        public string Label { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
    }

    public class UserAttendancePointDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalCount => PresentCount + AbsentCount;
    }

    public class VenueMeetingPointDto
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public int MeetingCount { get; set; }
        public double AveragePresentCount { get; set; }
        public double AverageAbsentCount { get; set; }
    }
}
