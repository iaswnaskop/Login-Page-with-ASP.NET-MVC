using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginPage.Entities;
using LoginPage.Models;
using LoginPage.Services.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;


namespace LoginPage.Controllers
{
    
    public class LoginViewController : Controller
    {
        private readonly ILoginService _loginService;

        public LoginViewController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public async Task<IActionResult> Register()
        {
            var roles = await _loginService.GetRoles();
            var model = new UserRegister
            {
                AvailableRoles = roles
            };
            return View(model);
        }

        public IActionResult Login()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> LoginIndex(string token)
        {
            
            if (string.IsNullOrEmpty(token))
            {
                // Μετά τσέκαρε το Authorization header
                var authorization = Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    token = authorization.Substring("Bearer ".Length).Trim();
                }
                else
                {
                    
                    token = Request.Cookies["token"];
                }
            }
            
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");
            
            var user = await _loginService.GetUserFromToken(token);
            if (user == null)
                return RedirectToAction("Login");
            
            // Αποθήκευσε το token σε cookie αν δεν υπάρχει
            if (!Request.Cookies.ContainsKey("token"))
            {
                Response.Cookies.Append("token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, 
                    SameSite = SameSiteMode.Strict
                });
            }

            var viewModel = new UserViewModel { CurrentUser = user };

            if (user.Role.Id == 1)
            {
                viewModel.AllUsers = await _loginService.GetAllUsers();
            }
            
            return View(viewModel); 
        }

       

    }
    [ApiController]
    [Route("api/")]

    public class LoginApiController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginApiController(ILoginService loginService)
        {
            _loginService = loginService;
        }

       
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            
            var user = await _loginService.Register(request);
            
            if (user == null)
                return BadRequest("User already exists");

            return Ok("Registration Successful");
        }

       
        
        [HttpPost("login")]
        public async Task <IActionResult> Login(UserLogin request)
        {
            var token  = await _loginService.Login(request);
            if (token is null)
                return BadRequest("Invalid credentials");
            
            return Ok(new {token = token});
        }
        
        

        
    }
}
