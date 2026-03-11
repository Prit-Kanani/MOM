using System.ComponentModel.DataAnnotations;

namespace MoM.Api.Models
{
    public class Meeting
    {
        public int Id { get; set; }

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

        public DateTime Date { get; set; }

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

        public byte[]? Logo { get; set; }

        public List<MeetingUser> MeetingUsers { get; set; } = new();
        public List<AgendaItem> Agendas { get; set; } = new();
        public List<ActionItem> ActionItems { get; set; } = new();
    }

    public class MeetingUser
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        public bool IsPresent { get; set; }
    }

    public class AgendaItem
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Topic { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Owner { get; set; } = string.Empty;
    }

    public class ActionItem
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Task { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Responsibility { get; set; } = string.Empty;

        public DateTime? Deadline { get; set; }
    }
}
