using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginPage.Context;
using LoginPage.Entities;
using LoginPage.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LoginPage.Services.Login;

public class LoginService(DataContext context,IConfiguration _configuration ) : ILoginService
{
    public async Task<string> Login(UserLogin request)
    {
        try
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if(user is null)
                return "User not found";
            
            // if (user.Email != request.Email)
            // {
            //     
            //     return "User not found";
            // }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, request.Password) ==
                PasswordVerificationResult.Failed)
            {
                return "Wrong password";
            }
            
            return CreateToken(user);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    public async Task<User> Register(UserRegister request)
    {
        try
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }

            var user = new User();
            
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);
            
            user.Name = request.Name;
            user.Email = request.Email;
            user.Password = hashedPassword;
            user.RoleId = request.RoleId;
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
     
    }

    public async Task<List<Role>> GetRoles()
    {
        try
        {
        return await context.Roles.ToListAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<User>> GetAllUsers()
    {
        try
        {
            return await context.Users.Include(u => u.Role).ToListAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<User> GetUserFromToken(string token)
    {
        try
        {
            // Validate input token
            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("Token is null or empty");
                return null;
            }

            // Remove "Bearer " prefix if present
            if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = token.Substring(7);
            }

            // Validate token format - JWT should have exactly 3 parts separated by dots
            var tokenParts = token.Split('.');
            if (tokenParts.Length != 3)
            {
                Console.WriteLine($"Invalid token format. Expected 3 parts, got {tokenParts.Length}");
                return null;
            }

            // Validate that none of the parts are empty
            if (tokenParts.Any(part => string.IsNullOrWhiteSpace(part)))
            {
                Console.WriteLine("Token contains empty segments");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!));
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration.GetValue<string>("AppSettings:Issuer"),
                ValidateAudience = true,
                ValidAudience = _configuration.GetValue<string>("AppSettings:Audience"),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) 
            {
                Console.WriteLine("NameIdentifier claim not found in token");
                return null;
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId)) 
            {
                Console.WriteLine("Invalid user ID format in token");
                return null;
            }

            return await context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Id == userId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error validating token: {e.Message}");
            return null;
        }
    }
    
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration.GetValue<string>("AppSettings:Token")!)
        );
            
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _configuration.GetValue<string>("AppSettings:Issuer"),
            audience: _configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );
        return new  JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}