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

        public byte[]? Logo { get; set; }

        public List<MeetingUserMap> UserMappings { get; set; } = new();
        public List<MeetingVenueMap> VenueMappings { get; set; } = new();
        public List<AgendaItem> Agendas { get; set; } = new();
        public List<ActionItem> ActionItems { get; set; } = new();
    }

    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string UserName { get; set; } = string.Empty;

        public List<MeetingUserMap> MeetingMappings { get; set; } = new();
    }

    public class Venue
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string VenueName { get; set; } = string.Empty;

        public List<MeetingVenueMap> MeetingMappings { get; set; } = new();
    }

    public class MeetingUserMap
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;
        public int UserId { get; set; }
        public AppUser User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = MeetingRoles.Attendee;

        public bool IsPresent { get; set; }
    }

    public class MeetingVenueMap
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public Meeting Meeting { get; set; } = null!;
        public int VenueId { get; set; }
        public Venue Venue { get; set; } = null!;
    }

    public static class MeetingRoles
    {
        public const string Facilitator = "Facilitator";
        public const string Chairperson = "Chairperson";
        public const string Secretary = "Secretary";
        public const string Attendee = "Attendee";
    }

    public class AgendaItem
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Topic { get; set; } = string.Empty;

        public int? OwnerUserId { get; set; }
        public AppUser? OwnerUser { get; set; }
    }

    public class ActionItem
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Task { get; set; } = string.Empty;

        public int? ResponsibilityUserId { get; set; }
        public AppUser? ResponsibilityUser { get; set; }

        public DateTime? Deadline { get; set; }
    }
}
