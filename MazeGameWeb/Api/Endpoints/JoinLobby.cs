using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using MazeGame.Api.Services;
using FluentValidation;

namespace MazeGame.Api.Endpoints
{
    public static class JoinLobby
    {
        public static void MapJoinLobbyEndpoint(this WebApplication app)
        {
            app.MapPost("/join", async (JoinGameRequest request, IValidator<JoinGameRequest> validator, GameDbContext db) =>
            {
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

                var game = await db.Games.FirstOrDefaultAsync(g => g.GameId == request.GameId);

                Debug.Assert(game.TurnNumber == 0, "TurnNumber should be 0");
                Debug.Assert(game.CurrentPlayer == PlayerType.Hider, "CurrentPlayer should be Hider");


                Guid joinerToken = Guid.NewGuid();
                PlayerType joinerRole;

                if (game.HiderToken == null)
                {
                    game.HiderToken = joinerToken;
                    joinerRole = PlayerType.Hider;
                }
                else
                {
                    game.SeekerToken = joinerToken;
                    joinerRole = PlayerType.Seeker;
                }

                //We assume the maze generated is valid, fully connected and that we can always place the 2nd player no matter the 1st player's position.
                game.Maze = MazeGenerator.Generate();

                PlacePlayers(game);

                game.GameStatus = GameStatus.Active;
                game.UpdatedAt = DateTimeOffset.UtcNow;

                await db.SaveChangesAsync();

                var response = new JoinGameResponse
                (
                    game.GameId,
                    joinerToken,
                    joinerRole,
                    game.GameStatus
                );

                return Results.Ok(response);

            });

        }

        //Uses a iterative approach to find a valid position for the players in the maze.
        private static void PlacePlayers(Game game)
        {
            var maze = game.Maze;
            Random rng = new Random();
            //Calculate Hider Position
            PlayerPosition hiderPos;
            while (true)
            {
                int Row = rng.Next(MazeGenerator.FirstIndex, MazeGenerator.LastIndex + 1);
                int Col = rng.Next(MazeGenerator.FirstIndex, MazeGenerator.LastIndex + 1);
                //Hider can only be placed on empty cell
                if (maze[Row][Col] == Cell.Empty)
                {
                    hiderPos = new PlayerPosition(Row, Col);
                    break;
                }
            }



            //Calculate Seeker Position
            PlayerPosition seekerPos;
            while (true)
            {
                int Row = rng.Next(MazeGenerator.FirstIndex, MazeGenerator.LastIndex + 1);
                int Col = rng.Next(MazeGenerator.FirstIndex, MazeGenerator.LastIndex + 1);
                //Seeker can only be placed on empty cell and not on the same, orthogonally adjacent or diagonally adjacent cell as the hider
                if (maze[Row][Col] == Cell.Empty && Math.Abs(Row - hiderPos.Row) > 1 && Math.Abs(Col - hiderPos.Column) > 1)
                {
                    seekerPos = new PlayerPosition(Row, Col);
                    break;
                }
            }

            game.HiderPosition = hiderPos;
            game.SeekerPosition = seekerPos;
        }
    }
}
