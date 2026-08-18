using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using MazeGame.Api.Services;
using MazeGame.Api.Validators;
using FluentValidation;
using System.Security.Claims;

namespace MazeGame.Api.Endpoints
{
    public static class JoinLobby
    {
        public static void MapJoinLobbyEndpoint(this WebApplication app)
        {
            app.MapPost("/join", async (HttpContext httpContext, JoinGameRequest request, IValidator<GameWithJoinGameRequest> validator, GameDbContext db) =>
            {
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Results.Unauthorized();
                }

                var game = await db.Games.FirstOrDefaultAsync(g => g.GameId == request.GameId);
                var gameWithRequest = new GameWithJoinGameRequest(game, request, userId);
                var validationResult = await validator.ValidateAsync(gameWithRequest);
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

                Debug.Assert(game.TurnNumber == 0, "TurnNumber should be 0");
                Debug.Assert(game.CurrentPlayer == PlayerType.Hider, "CurrentPlayer should be Hider");


                Guid joinerToken = Guid.NewGuid();
                PlayerType joinerRole;

                if (game.HiderToken == null)
                {
                    game.HiderToken = joinerToken;
                    game.HiderUserId = userId;
                    joinerRole = PlayerType.Hider;
                }
                else
                {
                    game.SeekerToken = joinerToken;
                    game.SeekerUserId = userId;
                    joinerRole = PlayerType.Seeker;
                }

                //We assume the maze generated is valid, fully connected and that we can always place the 2nd player no matter the 1st player's position.
                GameStarter.Start(game, new ModifiedWilsonMazeGenerator());

                await db.SaveChangesAsync();

                var response = new JoinGameResponse
                (
                    game.GameId,
                    joinerToken,
                    joinerRole,
                    game.GameStatus
                );

                return Results.Ok(response);

            }).RequireAuthorization();

        }

    }
}
