using MazeGame.Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MazeGame.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly TimeSpan _expiration;

        public TokenService(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");

            _key = jwtSection["Key"]
                ?? throw new InvalidOperationException("Jwt:Key configuration value is missing.");
            _issuer = jwtSection["Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer configuration value is missing.");
            _audience = jwtSection["Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience configuration value is missing.");

            int expirationMinutes = 60;
            if (int.TryParse(jwtSection["ExpirationMinutes"], out int configuredMinutes))
            {
                expirationMinutes = configuredMinutes;
            }
            _expiration = TimeSpan.FromMinutes(expirationMinutes);
        }

        public (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user)
        {
            var expiresAt = DateTimeOffset.UtcNow.Add(_expiration);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: expiresAt.UtcDateTime,
                signingCredentials: signingCredentials);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiresAt);
        }
    }
}
