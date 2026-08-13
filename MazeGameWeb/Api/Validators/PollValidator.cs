using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MazeGame.Api.Validators
{
    public class PollValidator : AbstractValidator<PollRequest>
    {
        /*
         * Poll validations.
         *  1. playerToken and gameId query parameters are required.
         *  2. Game exists.
         *  3. Player token is valid (Hider or Seeker).
         */
        public PollValidator(GameDbContext db)
        {
            RuleFor(x => x).CustomAsync(async (request, context, cancellationToken) =>
            {
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
                var game = await db.Games.FirstOrDefaultAsync(g => g.GameId == request.GameId, cancellationToken);
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
            });
        }
    }
}
