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
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public string? Venue { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalAttendees { get; set; }
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
        public string? Venue { get; set; }
        public string? Facilitator { get; set; }
        public string? Chairperson { get; set; }
        public string? Secretary { get; set; }
        public string? Logo { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalAttendees { get; set; }
        public List<MeetingUserDto> MeetingUsers { get; set; } = new();
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

        [MaxLength(200)]
        public string? Venue { get; set; }

        [MaxLength(200)]
        public string? Facilitator { get; set; }

        [MaxLength(200)]
        public string? Chairperson { get; set; }

        [MaxLength(200)]
        public string? Secretary { get; set; }

        public string? Logo { get; set; }

        public List<MeetingUserUpsertDto> MeetingUsers { get; set; } = new();
        public List<AgendaItemUpsertDto> Agendas { get; set; } = new();
        public List<ActionItemUpsertDto> ActionItems { get; set; } = new();
    }

    public class MeetingUserDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsPresent { get; set; }
    }

    public class MeetingUserUpsertDto
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        public bool IsPresent { get; set; }
    }

    public class AgendaItemDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }

    public class AgendaItemUpsertDto
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Owner { get; set; } = string.Empty;
    }

    public class ActionItemDto
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string Task { get; set; } = string.Empty;
        public string Responsibility { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
    }

    public class ActionItemUpsertDto
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Task { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Responsibility { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }
    }

    public class MeetingStatsDto
    {
        public int TotalMeetings { get; set; }
        public int TotalPresentAttendees { get; set; }
        public int TotalAbsentAttendees { get; set; }
        public List<MeetingVolumePointDto> MeetingVolume { get; set; } = new();
        public List<MeetingAttendancePointDto> MeetingAttendance { get; set; } = new();
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
}
