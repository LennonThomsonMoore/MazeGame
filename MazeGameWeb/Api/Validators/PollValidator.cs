using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;

namespace MazeGame.Api.Validators
{
    public class PollValidator : AbstractValidator<GameWithPollRequest>
    {
        /*
         * Poll validations.
         *  1. playerToken and gameId query parameters are required.
         *  2. Game exists.
         *  3. Player token is valid (Hider or Seeker).
         */
        public PollValidator()
        {
            RuleFor(x => x).CustomAsync(async (gameWithRequest, context, cancellationToken) =>
            {
                var request = gameWithRequest.pollRequest;
                var game = gameWithRequest.game;
                // 1. playerToken and gameId query parameters are required.
                if (request.PlayerToken == null || request.GameId == null)
                {
                    context.AddFailure(new ValidationFailure("PlayerToken", "playerToken and gameId query parameters are required.")
                    {
                        ErrorCode = "BadRequest"
                    });
                    return;
                }
                // 2. Game exists.
                if (game == null)
                {
                    context.AddFailure(new ValidationFailure("GameId", $"Game with id {request.GameId} not found.")
                    {
                        ErrorCode = "NotFound"
                    });
                    return;
                }
                // 3. Player token is valid (Hider or Seeker).
                if (game.HiderToken != request.PlayerToken && game.SeekerToken != request.PlayerToken)
                {
                    context.AddFailure(new ValidationFailure("PlayerToken", "CurrentPlayer must be either Hider or Seeker.")
                    {
                        ErrorCode = "BadRequest"
                    });
                    return;
                }
                await Task.CompletedTask;
            });
        }
    }
}
