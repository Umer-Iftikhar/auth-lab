using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthLab.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult GetPublic()
        {
            return Ok(new { message = "This is a public endpoint. No auth required." });
        }

        [HttpGet("private")]
        [Authorize]
        public IActionResult GetPrivate()
        {
            return Ok(new { message = "Auth successful! You are authorized to see this." });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetMe() 
        {
            var userClaims = User.Claims.Select(c => new 
            {
                c.Type,
                c.Value
            });

            var userInfo = new
            {
                Username = User.Identity?.Name, 
                UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value, 
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                AllClaims = userClaims,
                role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No Role"
            };

            return Ok(userInfo);
        }

        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult GetAdminData()
        {
            return Ok(new
            {
                Message = "Hello Admin! You have accessed a restricted endpoint.",
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AdminOrUser")]
        public IActionResult GetDashboardData()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "No Role";
            return Ok(new
            {
                Message = $"Hello {role}! Here is your dashboard data.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
