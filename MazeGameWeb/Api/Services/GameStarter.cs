using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    public static class GameStarter
    {
        public static Game Start(Game game, IMazeGenerator mazeGenerator)
        {
            var maze = mazeGenerator.Generate();
            game.GameStatus = GameStatus.Active;
            game.UpdatedAt = DateTimeOffset.UtcNow;
            game.Maze = maze;
            PlacePlayers(game);
            return game;
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
                int Row = rng.Next(Maze.FirstIndex, Maze.LastIndex + 1);
                int Col = rng.Next(Maze.FirstIndex, Maze.LastIndex + 1);
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
                int Row = rng.Next(Maze.FirstIndex, Maze.LastIndex + 1);
                int Col = rng.Next(Maze.FirstIndex, Maze.LastIndex + 1);
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
