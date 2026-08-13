using FluentValidation;
using FluentValidation.Results;
using MazeGame.Api.Contracts;
using MazeGame.Api.Data;
using MazeGame.Api.Models;
using MazeGame.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MazeGame.Api.Validators
{
    public class MoveValidator : AbstractValidator<MoveRequest>
    {

        /*
         * Move validations.
         *  1. Game exists.  

            2. Game is not already completed.  

            3. Player token is valid.  

            4. It is currently that player's turn.  

            5. Destination square is inside the maze.  

            6. Destination square is not a wall.  

            7. Movement is exactly one orthogonal square. 
         */
        public MoveValidator(GameDbContext db)
        {
            RuleFor(x => x).CustomAsync(async (request, context, cancellationToken) =>
            {
                // 1. Game exists.
                var game = await db.Games.FirstOrDefaultAsync(g => g.GameId == request.GameId, cancellationToken);
                if (game == null)
                {
                    context.AddFailure(new ValidationFailure("GameId", "Game does not exist.")
                    {
                        ErrorCode = "NotFound"
                    });
                    return;
                }
                // 2. Game is not already completed.
                if (game.GameStatus == GameStatus.Completed)
                {
                    context.AddFailure(new ValidationFailure("GameId", "Game already completed.")
                    {
                        ErrorCode = "Conflict"
                    });
                    return;
                }
                // 3. Player token is valid.
                if (request.PlayerToken != game.HiderToken && request.PlayerToken != game.SeekerToken)
                {
                    context.AddFailure(new ValidationFailure("PlayerToken", "Invalid player token.")
                    {
                        ErrorCode = "Unauthorized"
                    });
                    return;
                }
                // 4. It is currently that player's turn.
                PlayerType role = (request.PlayerToken == game.HiderToken) ? PlayerType.Hider : PlayerType.Seeker;
                if (game.CurrentPlayer != role)
                {
                    context.AddFailure(new ValidationFailure("PlayerToken", "Not this player's turn.")
                    {
                        ErrorCode = "Forbidden"
                    });
                    return;
                }
                // 5. Destination square is inside the maze.
                Debug.Assert(game.HiderPosition != null && game.SeekerPosition != null, "Player positions should not be null.");
                PlayerPosition position = (role == PlayerType.Hider) ? game.HiderPosition : game.SeekerPosition;
                PlayerPosition wantedPosition;
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
                    default:
                        context.AddFailure(new ValidationFailure("Direction", "Invalid direction.")
                        {
                            ErrorCode = "BadRequest"
                        });
                        return;
                }
                if (wantedPosition.Row > Maze.LastIndex    || 
                    wantedPosition.Column > Maze.LastIndex || 
                    wantedPosition.Row < Maze.FirstIndex   || 
                    wantedPosition.Column < Maze.FirstIndex
                    )
                {
                    context.AddFailure(new ValidationFailure("Direction", "Illegal move.")
                    {
                        ErrorCode = "BadRequest"
                    });
                    return;
                }

                Debug.Assert(wantedPosition.Row >= Maze.FirstIndex && wantedPosition.Row <= Maze.LastIndex, "Wanted position row is out of bounds.");
                Debug.Assert(wantedPosition.Column >= Maze.FirstIndex && wantedPosition.Column <= Maze.LastIndex, "Wanted position column is out of bounds.");
                Debug.Assert(game.Maze != null, "Maze should not be null.");
                // 6. Destination square is not a wall.
                if (game.Maze[wantedPosition.Row][wantedPosition.Column] == Cell.Wall)
                {
                    context.AddFailure(new ValidationFailure("Direction", "Illegal move.")
                    {
                        ErrorCode = "BadRequest"
                    });
                    return;
                }
            });

        }
    }
}
