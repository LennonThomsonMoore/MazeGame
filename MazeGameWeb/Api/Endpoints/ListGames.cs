using Microsoft.EntityFrameworkCore;
using MazeGame.Api.Data;
using MazeGame.Api.Models;

namespace MazeGame.Api.Endpoints
{
    public static class ListGames
    {

        private static readonly int MaxGames = 50;
        private static readonly int ExpiryMinutes = 10;
        public static void MapWaitingGames(this WebApplication app)
        {
            app.MapGet("/games/waiting", async (GameDbContext db) =>
            {
                var cutoff = DateTimeOffset.UtcNow.AddMinutes(-ExpiryMinutes);
                var waitingGames = (await db.Games
                    .Where(g => g.GameStatus == GameStatus.WaitingForPlayer)
                    .ToListAsync())
                    .Where(g => g.CreatedAt >= cutoff)
                    .OrderByDescending(g => g.CreatedAt)
                    .Take(MaxGames)
                    .Select(g => new { gameId = g.GameId, gameStatus = g.GameStatus })
                    .ToList();
                return Results.Ok(waitingGames);
            });
        }
    }
}
