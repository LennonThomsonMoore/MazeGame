using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    // Generates a perfect maze (a spanning tree with no loops) using Wilson's algorithm.
    // Logical cells live on even row/column indices (0, 2, 4, ... size-2), and the odd
    // indices between them represent the wall/passage that connects two adjacent cells.
    public class ModifiedWilsonMazeGenerator : IMazeGenerator
    {
        private const int size = MazeGenerator.size;
        private const int LastIndex = MazeGenerator.LastIndex;
        private const int InnerWallsToRemove = 5;

        private static readonly Random random = new Random();

        public Cell[][] Generate()
        {
            Cell[][] maze = GenerateWilson();

            // Ensure the start/end corners are open and connected to the maze.
            maze[0][0] = Cell.Empty;
            maze[LastIndex][LastIndex] = Cell.Empty;
            maze[LastIndex - 1][LastIndex] = Cell.Empty;

            RemoveRandomInnerWalls(maze, InnerWallsToRemove);

            return maze;
        }

        // Knocks down a handful of random interior walls to introduce loops/shortcuts
        // into the otherwise perfect maze produced by Wilson's algorithm.
        private static void RemoveRandomInnerWalls(Cell[][] maze, int count)
        {
            var innerWalls = new List<(int Row, int Col)>();
            for (int row = 1; row < LastIndex; row++)
            {
                for (int col = 1; col < LastIndex; col++)
                {
                    if (maze[row][col] == Cell.Wall)
                    {
                        innerWalls.Add((row, col));
                    }
                }
            }

            int wallsToRemove = Math.Min(count, innerWalls.Count);
            for (int i = 0; i < wallsToRemove; i++)
            {
                int index = random.Next(innerWalls.Count);
                var wall = innerWalls[index];
                maze[wall.Row][wall.Col] = Cell.Empty;
                innerWalls.RemoveAt(index);
            }
        }

        private static Cell[][] GenerateWilson()
        {
            Cell[][] maze = new Cell[size][];
            for (int row = 0; row < size; row++)
            {
                maze[row] = new Cell[size];
                for (int col = 0; col < size; col++)
                {
                    maze[row][col] = Cell.Wall;
                }
            }

            int logicalSize = size / 2;
            bool[,] inMaze = new bool[logicalSize, logicalSize];

            var unvisited = new List<(int Row, int Col)>();
            for (int row = 0; row < logicalSize; row++)
            {
                for (int col = 0; col < logicalSize; col++)
                {
                    unvisited.Add((row, col));
                }
            }

            // Seed the maze with a single random cell.
            var first = unvisited[random.Next(unvisited.Count)];
            inMaze[first.Row, first.Col] = true;
            maze[first.Row * 2][first.Col * 2] = Cell.Empty;
            unvisited.Remove(first);

            while (unvisited.Count > 0)
            {
                var walkStart = unvisited[random.Next(unvisited.Count)];
                var path = new List<(int Row, int Col)> { walkStart };
                var current = walkStart;

                // Perform a loop-erased random walk until we hit a cell already in the maze.
                while (!inMaze[current.Row, current.Col])
                {
                    var next = RandomNeighbor(current, logicalSize);

                    int loopStart = path.IndexOf(next);
                    if (loopStart >= 0)
                    {
                        // Erase the loop formed by revisiting a cell already on the path.
                        path.RemoveRange(loopStart + 1, path.Count - loopStart - 1);
                    }
                    else
                    {
                        path.Add(next);
                    }

                    current = next;
                }

                // Carve the resulting loop-erased path into the maze.
                for (int i = 0; i < path.Count; i++)
                {
                    var cell = path[i];
                    inMaze[cell.Row, cell.Col] = true;
                    maze[cell.Row * 2][cell.Col * 2] = Cell.Empty;
                    unvisited.Remove(cell);

                    if (i > 0)
                    {
                        var previous = path[i - 1];
                        int wallRow = previous.Row * 2 + (cell.Row - previous.Row);
                        int wallCol = previous.Col * 2 + (cell.Col - previous.Col);
                        maze[wallRow][wallCol] = Cell.Empty;
                    }
                }
            }

            return maze;
        }

        private static (int Row, int Col) RandomNeighbor((int Row, int Col) cell, int logicalSize)
        {
            var neighbors = new List<(int Row, int Col)>(4);

            if (cell.Row > 0)
                neighbors.Add((cell.Row - 1, cell.Col));
            if (cell.Row < logicalSize - 1)
                neighbors.Add((cell.Row + 1, cell.Col));
            if (cell.Col > 0)
                neighbors.Add((cell.Row, cell.Col - 1));
            if (cell.Col < logicalSize - 1)
                neighbors.Add((cell.Row, cell.Col + 1));

            return neighbors[random.Next(neighbors.Count)];
        }
    }
}
