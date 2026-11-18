using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Data;

/// <summary>
/// Applies pending migrations and loads demo data from Data/Seed/*.json
/// the first time the database is created.
/// </summary>
public static class DbSeeder
{
    public static void Initialize(BookLyticsDbContext db, string contentRootPath)
    {
        db.Database.Migrate();

        if (db.Users.Any())
            return; // already seeded

        var seedDir = Path.Combine(contentRootPath, "Data", "Seed");
        var users = Load<User>(Path.Combine(seedDir, "users.json"));
        var books = Load<Book>(Path.Combine(seedDir, "books.json"));

        // Drop any seed books whose owner doesn't exist so the FK constraint holds
        var userIds = users.Select(u => u.Id).ToHashSet();
        books = books.Where(b => userIds.Contains(b.OwnerId)).ToList();

        using var tx = db.Database.BeginTransaction();
        db.Users.AddRange(users);
        db.Books.AddRange(books);
        db.SaveChanges();
        tx.Commit();
    }

    private static List<T> Load<T>(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path)) ?? new List<T>()
            : new List<T>();
}
