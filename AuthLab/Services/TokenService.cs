using AuthLab.DTOs;
using AuthLab.Models;
using AuthLab.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthLab.Services
{
    public class TokenService
    {
        private readonly JwtConfig _jwtConfig;
        public TokenService(IOptions<JwtConfig> options)
        {
            _jwtConfig = options.Value;
        }
        public AuthResponseDto GenerateToken(ApplicationUser user)
        {
            // Create claims based on the user information
            // A Claim is a statement about the user (e.g., user ID, username, email)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));

            var expiresAt = DateTime.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes);

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // HMAC SHA256 creates a signature based on the header and payload of the token, using the secret key. 

            // Create the token descriptor which includes the claims, expiration time, and signing credentials
            // The token descriptor is a blueprint for the JWT token. It defines the issuer, subject (claims), expiration time, and signing credentials.
            // SecurityTokenDescriptor is a class that describes the properties of the token to be created. It includes the issuer, subject (claims), expiration time, and signing credentials.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtConfig.Issuer,
                Subject = new ClaimsIdentity(claims),
                Audience = _jwtConfig.Audience,
                Expires = expiresAt,
                SigningCredentials = credentials
            };

            // Create the token using the JwtSecurityTokenHandler
            // JwtSecurityTokenHandler is a class that provides methods for creating and validating JWT tokens.
            // CreateToken method generates a JWT token based on the provided token descriptor. It creates the token by combining the header, payload (claims), and signature.
            var handler = new JwtSecurityTokenHandler();

            // The CreateToken method generates a JWT token based on the provided token descriptor. It creates the token by combining the header, payload (claims), and signature.
            var token = handler.CreateToken(tokenDescriptor);

            // WriteToken method serializes the token into a string format that can be sent to clients. It converts the token object into a compact, URL-safe string representation.
            var tokenString = handler.WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                ExpiresAt = expiresAt
            };
        }
    }
}
