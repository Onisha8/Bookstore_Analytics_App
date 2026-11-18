using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Data;

public class BookLyticsDbContext : DbContext
{
    public BookLyticsDbContext(DbContextOptions<BookLyticsDbContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<Book>(book =>
        {
            book.HasOne(b => b.Owner)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes that back the per-user queries and analytics groupings
            book.HasIndex(b => b.OwnerId);
            book.HasIndex(b => new { b.OwnerId, b.Genre });
        });
    }
}
