using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using MazeGame.Api.Data;

namespace MazeGame.Api.Validators
{
    public class JoinGameRequestValidator : AbstractValidator<JoinGameRequest>
    {
        public JoinGameRequestValidator(GameDbContext db)
        {
            RuleFor(x => x).CustomAsync(async (request, context, cancellationToken) =>
            {
                var game = await db.Games.FirstOrDefaultAsync(g => g.GameId == request.GameId, cancellationToken);  
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
            });
        }
    }
}
