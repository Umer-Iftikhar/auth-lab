using AuthLab.Models;

namespace AuthLab.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateRefreshToken(string userId);
        Task<RefreshToken?> ValidateRefreshToken(string token);
        Task RevokeRefreshToken(RefreshToken token);
    }
}
