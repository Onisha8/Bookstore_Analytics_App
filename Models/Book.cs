using System.ComponentModel.DataAnnotations;

namespace BookApp.Models;

public class Book
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Author { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Genre { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string Review { get; set; } = string.Empty;

    public bool Liked { get; set; }

    // Foreign key to User.Id; navigation property lets EF Core model the relationship
    public int OwnerId { get; set; }
    public User? Owner { get; set; }
}
