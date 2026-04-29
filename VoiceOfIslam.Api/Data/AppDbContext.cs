using Microsoft.EntityFrameworkCore;
using VoiceOfIslam.Shared.Models;

namespace VoiceOfIslam.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AudioStream> AudioStreams { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AudioStream>().ToTable("AudioStreams");
        }
    }
}
