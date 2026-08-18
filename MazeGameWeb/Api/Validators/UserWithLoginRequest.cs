using MazeGame.Api.Models;
using MazeGame.Api.Contracts;

namespace MazeGame.Api.Validators
{
    public record UserWithLoginRequest(
        User? user,
        LoginRequest loginRequest
    );
}
