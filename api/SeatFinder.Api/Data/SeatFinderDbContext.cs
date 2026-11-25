using Microsoft.EntityFrameworkCore;

namespace SeatFinder.Api.Data;

public class SeatFinderDbContext(DbContextOptions<SeatFinderDbContext> options) : DbContext(options)
{
    public DbSet<SeatPreference> SeatPreferences => Set<SeatPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeatPreference>(entity =>
        {
            // The app always writes id 1 itself, so Postgres shouldn't hand out ids.
            entity.Property(p => p.Id).ValueGeneratedNever();

            // Row labels are short ("A".."M" today); a limit keeps junk out at the database level too.
            entity.Property(p => p.PreferredRowFrom).HasMaxLength(3);
            entity.Property(p => p.PreferredRowTo).HasMaxLength(3);
        });
    }
}
