using System.Collections.Generic;
using System.Linq;
using BookApp.Data;
using BookApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApp.Services;

public class EfBookRepository : IBookRepository
{
    private readonly BookLyticsDbContext _db;

    public EfBookRepository(BookLyticsDbContext db) => _db = db;

    public IEnumerable<Book> GetAll() =>
        _db.Books.AsNoTracking().ToList();

    // Filter runs in SQL (WHERE OwnerId = @p) instead of loading every book into memory
    public IEnumerable<Book> GetByOwner(int ownerId) =>
        _db.Books.AsNoTracking()
                 .Where(b => b.OwnerId == ownerId)
                 .ToList();

    public Book? GetById(int id) =>
        _db.Books.AsNoTracking().FirstOrDefault(b => b.Id == id);

    public void Add(Book book)
    {
        book.Id = 0; // let the database generate the key
        _db.Books.Add(book);
        _db.SaveChanges();
    }

    public void Update(Book book)
    {
        // Only update a book that belongs to the same owner, so a forged
        // form post can't overwrite another user's record
        var existing = _db.Books.FirstOrDefault(b => b.Id == book.Id && b.OwnerId == book.OwnerId);
        if (existing is null) return;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Genre = book.Genre;
        existing.Rating = book.Rating;
        existing.Review = book.Review;
        existing.Liked = book.Liked;
        _db.SaveChanges();
    }

    public void Delete(int id)
    {
        var existing = _db.Books.Find(id);
        if (existing is null) return;
        _db.Books.Remove(existing);
        _db.SaveChanges();
    }
}
