using MazeGame.Api.Models;
using MazeGame.Api.Contracts;

namespace MazeGame.Api.Validators
{
    public record GameWithPollRequest(
        Game? game,
        PollRequest pollRequest
    );
}
