using FluentValidation;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using MazeGame.Api.Contracts;
using Microsoft.EntityFrameworkCore;

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
                /*
                 * #### Processing

                    1. Update the player's position.  

                    2. Check for capture.  

                    3. If both players occupy the same square:  
                    - Mark game as completed.  

                    - Set `winner = seeker`.  
                    4. Advance turn state.  

                    5. After the seeker finishes their move:  
                    - Increment the complete turn counter.  
                    6. If 100 complete turns have elapsed without capture:  
                    - Mark game as completed.  

                    - Set `winner = hider`.  
                    7. Persist all changes atomically.  
                  */
                PlayerType role = (game.HiderToken == request.PlayerToken) ? PlayerType.Hider : PlayerType.Seeker;
                PlayerPosition position = (role == PlayerType.Hider) ? game.HiderPosition : game.SeekerPosition;
                PlayerPosition wantedPosition = position;
                switch (request.Direction)
                {
                    case Direction.North:
                        wantedPosition = new PlayerPosition(position.Row - 1, position.Column);
                        break;
                    case Direction.South:
                        wantedPosition = new PlayerPosition(position.Row + 1, position.Column);
                        break;
                    case Direction.West:
                        wantedPosition = new PlayerPosition(position.Row, position.Column - 1);
                        break;
                    case Direction.East:
                        wantedPosition = new PlayerPosition(position.Row, position.Column + 1);
                        break;
                }
                //Updates player position
                if (role == PlayerType.Hider)
                {
                    game.HiderPosition = wantedPosition;
                }
                else
                {
                    game.SeekerPosition = wantedPosition;
                }

                //Checkes for capture
                if (game.SeekerPosition.Equals(game.HiderPosition))
                {
                    game.GameStatus = GameStatus.Completed;
                    game.Winner = PlayerType.Seeker;
                }
                else
                {
                    game.CurrentPlayer = (role == PlayerType.Hider) ? PlayerType.Seeker : PlayerType.Hider;
                    if (role == PlayerType.Seeker)
                    {
                        game.TurnNumber++;
                        if (game.TurnNumber >= 100)
                        {
                            game.GameStatus = GameStatus.Completed;
                            game.Winner = PlayerType.Hider;
                        }
                    }
                }

                game.UpdatedAt = DateTimeOffset.UtcNow;

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
