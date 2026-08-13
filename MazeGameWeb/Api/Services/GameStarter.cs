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

        //Places players in game where players cannot be place diagonally or othorgonally adjacent or inside walls.
        private static void PlacePlayers(Game game)
        {
            // make a list of all empty cells
            // pick one at random and place the hider there
            // then remove all cells that are adjacent to the hider from the list
            // pick one at random for the seeker
            var emptyCells = new List<(int x, int y)>();
            for (int x = 0; x < game.Maze!.Length; x++)
            {
                for (int y = 0; y < game.Maze[x].Length; y++)
                {
                    if (game.Maze[x][y] == Cell.Empty)
                    {
                        emptyCells.Add((x, y));
                    }
                }
            }

            Random rng = new Random();
            var hiderIndex = rng.Next(emptyCells.Count);
            var hiderPos = emptyCells[hiderIndex];
            emptyCells.RemoveAll(cell => Math.Abs(cell.x - hiderPos.x) <= 1 && Math.Abs(cell.y - hiderPos.y) <= 1);

            var seekerIndex = rng.Next(emptyCells.Count);
            var seekerPos = emptyCells[seekerIndex];

            game.HiderPosition = new PlayerPosition(hiderPos.x, hiderPos.y);
            game.SeekerPosition = new PlayerPosition(seekerPos.x, seekerPos.y);
        }
    }
}
