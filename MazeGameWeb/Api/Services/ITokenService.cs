using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    public interface ITokenService
    {
        (string Token, DateTimeOffset ExpiresAt) GenerateToken(User user);
    }
}
