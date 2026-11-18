using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BookApp.Data;
using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Services;

public class EfUserService : IUserService
{
    private readonly BookLyticsDbContext _db;

    public EfUserService(BookLyticsDbContext db) => _db = db;

    public IEnumerable<User> GetAll() => _db.Users.AsNoTracking().ToList();

    public User? GetById(int id) => _db.Users.AsNoTracking().SingleOrDefault(u => u.Id == id);

    public User? Validate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || password is null)
            return null;

        var normalized = username.Trim().ToLower();
        var user = _db.Users.AsNoTracking()
                            .SingleOrDefault(u => u.Username.ToLower() == normalized);
        if (user is null) return null;

        // NOTE: unsalted SHA-256 kept for compatibility with the seed data;
        // see README "Known Limitations"
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return hash == user.PasswordHash ? user : null;
    }
}
