using Microsoft.EntityFrameworkCore;
using Music.Library.Repository.Model;

namespace Music.Library.Repository;

public class LibraryDbContext(DbContextOptions<LibraryDbContext> dbContextOptions) : DbContext(dbContextOptions)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<Libraries>().HasKey(s => s.Id);
        modelBuilder.Entity<Libraries>().Property(s => s.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<Libraries>().ToTable("Libraries");
        
        modelBuilder.Entity<LibrarySongs>().HasKey(s => s.Id);
        modelBuilder.Entity<LibrarySongs>().Property(s => s.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<LibrarySongs>()
            .Property(u => u.DataAggiunta)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        modelBuilder.Entity<LibrarySongs>().ToTable("LibrarySongs");
        
    }
    
    public DbSet<Libraries> LibrariesEnumerable { get; set; }
    public DbSet<LibrarySongs> LibrarySongsEnumerable { get; set; }
}