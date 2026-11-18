using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookApp.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    // One user owns many books (1:N)
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
