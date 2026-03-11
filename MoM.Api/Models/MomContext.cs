using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;

namespace MoM.Api.Models
{
    public class MomContext : DbContext
    {
        public MomContext(DbContextOptions<MomContext> options) : base(options) { }

        public DbSet<Meeting> Meetings { get; set; }
        public DbSet<MeetingUser> MeetingUsers { get; set; }
        public DbSet<AgendaItem> AgendaItems { get; set; }
        public DbSet<ActionItem> ActionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.MeetingUsers)
                .WithOne()
                .HasForeignKey(u => u.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.Agendas)
                .WithOne()
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meeting>()
                .HasMany(m => m.ActionItems)
                .WithOne()
                .HasForeignKey(a => a.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
