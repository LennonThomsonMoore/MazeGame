using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Validators;
using FluentValidation;
using System.Security.Claims;

namespace MazeGame.Api.Endpoints
{
    public static class CreateGame
    {
        public static void MapCreateGameEndpoint(this WebApplication app)
        {
            app.MapPost("/create", async (HttpContext httpContext, IValidator<ClaimsPrincipal> validator, GameDbContext db) =>
            {
                var validationResult = await validator.ValidateAsync(httpContext.User);
                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.First();
                    return firstError.ErrorCode switch
                    {
                        "Unauthorized" => Results.Unauthorized(),
                        _ => Results.BadRequest(new { error = firstError.ErrorMessage, errorCode = firstError.ErrorCode })
                    };
                }

                var userId = Guid.Parse(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var hostToken = Guid.NewGuid();

                bool isHiderFirst = (hostToken.ToByteArray()[0] & 1) == 0; // Should be 50 % chance to be hider or seeker
                var role = (isHiderFirst) ? PlayerType.Hider : PlayerType.Seeker;
                var game = (isHiderFirst) ? Game.CreateHiderGame(hostToken, userId) : Game.CreateSeekerGame(hostToken, userId);

                // Save the game to the database
                db.Games.Add(game);
                await db.SaveChangesAsync();

                // Generate the response
                CreateGameResponse response = new CreateGameResponse
                (
                    game.GameId,
                    hostToken,
                    role,
                    GameStatus.WaitingForPlayer
                );

                return Results.Ok(response);
            }).RequireAuthorization();
        }
    }
}
