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

        modelBuilder.Entity<OutboxMessage>().HasKey(o => o.Id);
        modelBuilder.Entity<OutboxMessage>().Property(o => o.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<OutboxMessage>()
            .Property(o => o.CreatedAt)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        modelBuilder.Entity<OutboxMessage>()
            .Property(o => o.ProcessedAt)
            .HasConversion(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
        // Indice sulle colonne usate dal poller per selezionare i messaggi da pubblicare
        modelBuilder.Entity<OutboxMessage>().HasIndex(o => new { o.Status, o.CreatedAt });
        modelBuilder.Entity<OutboxMessage>().ToTable("OutboxMessages");

    }

    public DbSet<Libraries> LibrariesEnumerable { get; set; }
    public DbSet<LibrarySongs> LibrarySongsEnumerable { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}