using System.Security.Claims;
using MazeGame.Api.Data;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using MazeGame.Api.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace MazeGame.Api.Endpoints
{
    public static class ListMeGames
    {
        public static void MapListMeGamesEndpoint(this WebApplication app)
        {
            app.MapGet("/me/games", async (HttpContext httpContext, IValidator<ClaimsPrincipal> validator, GameDbContext db) =>
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

                var games = (await db.Games
                    .Where(g => g.HiderUserId == userId || g.SeekerUserId == userId)
                    .ToListAsync())
                    .OrderByDescending(g => g.UpdatedAt)
                    .Select(g => new MyGameResponse(
                        g.GameId,
                        g.HiderUserId == userId ? PlayerType.Hider : PlayerType.Seeker,
                        g.GameStatus,
                        g.Winner,
                        g.HiderUserId == userId ? g.HiderToken : g.SeekerToken,
                        g.CreatedAt,
                        g.UpdatedAt))
                    .ToList();

                return Results.Ok(games);

            }).RequireAuthorization();
        }
    }
}

