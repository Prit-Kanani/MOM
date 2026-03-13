using Microsoft.EntityFrameworkCore;

namespace MoM.Api.Models
{
    public class MomContext : DbContext
    {
        public MomContext(DbContextOptions<MomContext> options) : base(options) { }

        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<AuthUser> AuthUsers { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<MeetingUserMap> MeetingUserMaps { get; set; }
        public DbSet<MeetingVenueMap> MeetingVenueMaps { get; set; }
        public DbSet<AgendaItem> AgendaItems { get; set; }
        public DbSet<ActionItem> ActionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<AuthUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<AuthUser>()
                .HasMany<Meeting>()
                .WithOne(m => m.CreatedByAuthUser)
                .HasForeignKey(m => m.CreatedByAuthUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venue>()
                .HasIndex(v => v.VenueName)
                .IsUnique();

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.UserMappings)
                .WithOne(m => m.Meeting)
                .HasForeignKey(m => m.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.VenueMappings)
                .WithOne(m => m.Meeting)
                .HasForeignKey(m => m.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.MeetingMappings)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Venue>()
                .HasMany(v => v.MeetingMappings)
                .WithOne(m => m.Venue)
                .HasForeignKey(m => m.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MeetingUserMap>()
                .HasIndex(m => new { m.MeetingId, m.UserId, m.Role })
                .IsUnique();

            modelBuilder.Entity<MeetingVenueMap>()
                .HasIndex(m => new { m.MeetingId, m.VenueId })
                .IsUnique();

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.Agendas)
                .WithOne()
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AgendaItem>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.ActionItems)
                .WithOne()
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActionItem>()
                .HasOne(a => a.ResponsibilityUser)
                .WithMany()
                .HasForeignKey(a => a.ResponsibilityUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
