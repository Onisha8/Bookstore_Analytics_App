using System.Collections.Generic;
using BookApp.Models;

namespace BookApp.Services;

public interface IBookRepository
{
    IEnumerable<Book> GetAll();
    IEnumerable<Book> GetByOwner(int ownerId);
    Book? GetById(int id);
    void Add(Book book);
    void Update(Book book);
    void Delete(int id);
}
