using System;
using System.Collections.Generic;

namespace LoginPage.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public int? RoleId { get; set; }
    public Role? Role { get; set; }
  
}
public class UserViewModel
{
    public User CurrentUser { get; set; }
    public List<User>? AllUsers { get; set; }
}