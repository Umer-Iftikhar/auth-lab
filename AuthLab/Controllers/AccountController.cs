using AuthLab.DTOs;
using AuthLab.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {

            var user = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Name
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(errors);
            }


            return Created("", new { id = user.Id, message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid Email or Password");
            }

            var password = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!password)
            {
                return Unauthorized("Invalid Email or Password");
            }

            return Ok(new { id = user.Id, email = user.Email, role = "User", message = "Login successful" });
        }
    }
}
