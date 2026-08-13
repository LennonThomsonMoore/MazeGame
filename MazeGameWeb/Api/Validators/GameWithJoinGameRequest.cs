using MazeGame.Api.Models;
using MazeGame.Api.Contracts;

namespace MazeGame.Api.Validators
{
    public record GameWithJoinGameRequest(
        Game? game,
        JoinGameRequest joinGameRequest
    );
}
