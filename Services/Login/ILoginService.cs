using LoginPage.Entities;
using LoginPage.Models;

namespace LoginPage.Services.Login;

public interface ILoginService
{
    Task<string> Login(UserLogin request);
    Task<User> Register(UserRegister request);
    Task<User> GetUserFromToken(string token);
    Task<List<Role>> GetRoles();
    Task<List<User>> GetAllUsers();
}