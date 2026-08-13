using MazeGame.Api.Models;
using MazeGame.Api.Contracts;

namespace MazeGame.Api.Validators
{
    public record GameWithMoveRequest(
        Game game,
        MoveRequest moveRequest
    );
}
