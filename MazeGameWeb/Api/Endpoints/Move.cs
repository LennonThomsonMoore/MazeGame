using FluentValidation;
using MazeGame.Api.Contracts;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using MazeGame.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace MazeGame.Api.Endpoints
{
    public static class Move
    {
        public static void MapMoveEndpoint(this WebApplication app)
        {
            app.MapPost("/move", async (MoveRequest request, IValidator<MoveRequest> validator, GameDbContext db) => {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return firstError.ErrorCode switch
                    {
                        "NotFound" => Results.NotFound(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode }),
                        "Conflict" => Results.Conflict(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode }),
                        _ => Results.BadRequest(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode })
                    };
                }
                var game = db.Games.FirstOrDefault(g => g.GameId == request.GameId);

                PlayerType role = (game.HiderToken == request.PlayerToken) ? PlayerType.Hider : PlayerType.Seeker;

                GameUpdater.update(request.Direction, role, game);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return Results.Conflict(new
                    {
                        error = "The game state changed before this move could be applied. Please refresh and try again.",
                        errorCode = "Conflict"
                    });
                }

                var response = new MoveResponse
                (
                    game.GameId,
                    game.CurrentPlayer,
                    game.TurnNumber,
                    game.GameStatus,
                    game.Winner
                );
                return Results.Ok(response);

            });
            

        }
    }
}
