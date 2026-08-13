using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    public class GameUpdater
    {
        public static void update(Direction direction, PlayerType role, Game game)
        {
            PlayerPosition position = (role == PlayerType.Hider) ? game.HiderPosition : game.SeekerPosition;
            PlayerPosition wantedPosition = position;
            switch (direction)
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
                // If both players occupy the same square:  
                // - Mark game as completed.
                // - Set `winner = seeker`.  
                game.GameStatus = GameStatus.Completed;
                game.Winner = PlayerType.Seeker;
            }
            else
            {
                // If 100 complete turns have elapsed without capture:  
                // - Mark game as completed.
                // - Set `winner = hider`. 
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
        }
    }
}
