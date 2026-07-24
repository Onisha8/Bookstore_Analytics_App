using System.ComponentModel.DataAnnotations;

namespace BookApp.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Rating { get; set; }          // 1–5
    public string Review { get; set; } = string.Empty;
    public bool Liked { get; set; }

    // Adding a New link to User.Id
    public int OwnerId { get; set; }
}