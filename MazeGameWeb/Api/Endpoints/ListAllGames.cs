using Microsoft.EntityFrameworkCore;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGame.Api.Validators;
using FluentValidation;
using System.Security.Claims;

namespace MazeGame.Api.Endpoints
{
    public static class ListAllGames
    {

        private static readonly int MaxGames = 50;
        private static readonly int ExpiryMinutes = 10;
        public static void MapWaitingGames(this WebApplication app)
        {
            app.MapGet("/games/waiting", async (HttpContext httpContext, IValidator<ClaimsPrincipal> validator, GameDbContext db) =>
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

                var cutoff = DateTimeOffset.UtcNow.AddMinutes(-ExpiryMinutes);
                var waitingGames = (await db.Games
                    .Where(g => g.GameStatus == GameStatus.WaitingForPlayer)
                    .ToListAsync())
                    .Where(g => g.CreatedAt >= cutoff)
                    .Where(g => g.HiderUserId != userId && g.SeekerUserId != userId)
                    .OrderByDescending(g => g.CreatedAt)
                    .Take(MaxGames)
                    .Select(g => new WaitingGameResponse(g.GameId, g.GameStatus))
                    .ToList();
                return Results.Ok(waitingGames);
            }).RequireAuthorization();
        }
    }
}
