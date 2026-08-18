using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;

namespace MazeGame.Api.Endpoints
{
    public static class CreateGame
    {
        public static void MapCreateGameEndpoint(this WebApplication app)
        {
            app.MapPost("/create", async (GameDbContext db) =>
            {
                var hostToken = Guid.NewGuid();

                bool isHiderFirst = (hostToken.ToByteArray()[0] & 1) == 0; // Should be 50 % chance to be hider or seeker
                var role = (isHiderFirst) ? PlayerType.Hider : PlayerType.Seeker;
                var game = (isHiderFirst) ? Game.CreateHiderGame(hostToken) : Game.CreateSeekerGame(hostToken);

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
