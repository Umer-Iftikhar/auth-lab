using AuthLab.DTOs;
using AuthLab.Models;

namespace AuthLab.Services.Interfaces
{
    public interface ITokenService
    {
        public AuthResponseDto GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
