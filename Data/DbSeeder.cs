using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BookApp.Models;
using Microsoft.Data.Sqlite;
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
        EnsureDatabaseDirectory(db);
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

    // On Azure the database lives outside the deployed app folder (e.g. /home/data)
    // so it survives redeploys; make sure that folder exists before SQLite opens it.
    private static void EnsureDatabaseDirectory(BookLyticsDbContext db)
    {
        var dataSource = new SqliteConnectionStringBuilder(db.Database.GetConnectionString()).DataSource;
        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    private static List<T> Load<T>(string path) =>
        File.Exists(path)
            ? JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path)) ?? new List<T>()
            : new List<T>();
}
