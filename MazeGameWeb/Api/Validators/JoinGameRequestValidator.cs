using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;

namespace MazeGame.Api.Validators
{
    public class JoinGameRequestValidator : AbstractValidator<GameWithJoinGameRequest>
    {
        public JoinGameRequestValidator()
        {
            RuleFor(x => x).CustomAsync(async (gameWithRequest, context, cancellationToken) =>
            {
                var game = gameWithRequest.game;
                if (game == null)
                {

                    context.AddFailure(new ValidationFailure("GameId", "Game does not exist.")
                    {
                        ErrorCode = "NotFound"
                    });
                    return;
                }
                if (game.HiderToken != null && game.SeekerToken != null)
                {
                    context.AddFailure(new ValidationFailure("GameId", "Game is already full.")
                    {
                        ErrorCode = "Conflict"
                    });
                    return;
                }

                if (game.GameStatus == GameStatus.Active)
                {
                    context.AddFailure(new ValidationFailure("GameId", "Game is already active.")
                    {
                        ErrorCode = "Conflict"
                    });
                    return;
                }

                if (game.GameStatus == GameStatus.Completed)
                {
                    context.AddFailure(new ValidationFailure("GameId", "Game is already completed.")
                    {
                        ErrorCode = "Conflict"
                    });
                    return;
                }

                var creatorUserId = game.HiderUserId ?? game.SeekerUserId;
                if (creatorUserId != null && creatorUserId == gameWithRequest.userId)
                {
                    context.AddFailure(new ValidationFailure("GameId", "You cannot join a game you created.")
                    {
                        ErrorCode = "Conflict"
                    });
                    return;
                }
            });
        }
    }
}
