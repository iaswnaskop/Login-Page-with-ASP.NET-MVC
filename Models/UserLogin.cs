using System.ComponentModel.DataAnnotations;

namespace LoginPage.Models;

public class UserLogin
{
    public Guid Id { get; set; }
    [Required]
    public string Email { get; set; } 
    [Required]

    public string Password { get; set; } 
}

