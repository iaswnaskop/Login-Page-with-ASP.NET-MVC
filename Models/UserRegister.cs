using System.ComponentModel.DataAnnotations;
using LoginPage.Entities;

namespace LoginPage.Models;

public class UserRegister
{
    [Required]
    public string Name { get; set; } 
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public int  RoleId { get; set; }
    public IEnumerable<Role> AvailableRoles { get; set; } = new List<Role>();
}