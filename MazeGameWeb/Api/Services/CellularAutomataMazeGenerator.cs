using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    // Generates a maze using a cellular automata approach (similar to Conway's Game of Life rules
    // adapted for maze generation). Starting from a random noise grid, the grid is repeatedly
    // smoothed using birth/survival rules until it stabilizes into cave-like, maze-like corridors.
    public class CellularAutomataMazeGenerator : IMazeGenerator
    {
        private const int Size = MazeGenerator.size;
        private const int FirstIndex = MazeGenerator.FirstIndex;
        private const int LastIndex = MazeGenerator.LastIndex;

        // Probability that a cell starts as a wall.
        private const double WallProbability = 0.45;

        // A wall cell survives if it has at least this many wall neighbors.
        private const int SurvivalThreshold = 4;

        // An open cell becomes a wall if it has at least this many wall neighbors.
        private const int BirthThreshold = 5;

        private const int Iterations = 5;

        private static readonly Random random = new Random();

        public Cell[][] Generate()
        {
            Cell[][] maze = InitializeRandomGrid();

            for (int i = 0; i < Iterations; i++)
            {
                maze = Step(maze);
            }

            // Ensure the start/end corners are open and connected to the maze.
            maze[FirstIndex][FirstIndex] = Cell.Empty;
            maze[LastIndex][LastIndex] = Cell.Empty;
            maze[LastIndex - 1][LastIndex] = Cell.Empty;
            maze[LastIndex][LastIndex - 1] = Cell.Empty;

            ConnectRegions(maze);

            return maze;
        }

        private static Cell[][] InitializeRandomGrid()
        {
            Cell[][] maze = new Cell[Size][];
            for (int row = 0; row < Size; row++)
            {
                maze[row] = new Cell[Size];
                for (int col = 0; col < Size; col++)
                {
                    // Force the border to be walls so the maze is enclosed.
                    if (row == FirstIndex || row == LastIndex || col == FirstIndex || col == LastIndex)
                    {
                        maze[row][col] = Cell.Wall;
                    }
                    else
                    {
                        maze[row][col] = random.NextDouble() < WallProbability ? Cell.Wall : Cell.Empty;
                    }
                }
            }

            return maze;
        }

        private static Cell[][] Step(Cell[][] maze)
        {
            Cell[][] next = new Cell[Size][];
            for (int row = 0; row < Size; row++)
            {
                next[row] = new Cell[Size];
                for (int col = 0; col < Size; col++)
                {
                    if (row == FirstIndex || row == LastIndex || col == FirstIndex || col == LastIndex)
                    {
                        next[row][col] = Cell.Wall;
                        continue;
                    }

                    int wallNeighbors = CountWallNeighbors(maze, row, col);

                    if (maze[row][col] == Cell.Wall)
                    {
                        next[row][col] = wallNeighbors >= SurvivalThreshold ? Cell.Wall : Cell.Empty;
                    }
                    else
                    {
                        next[row][col] = wallNeighbors >= BirthThreshold ? Cell.Wall : Cell.Empty;
                    }
                }
            }

            return next;
        }

        private static int CountWallNeighbors(Cell[][] maze, int row, int col)
        {
            int count = 0;
            for (int dRow = -1; dRow <= 1; dRow++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    if (dRow == 0 && dCol == 0)
                        continue;

                    int neighborRow = row + dRow;
                    int neighborCol = col + dCol;

                    if (neighborRow < 0 || neighborRow >= Size || neighborCol < 0 || neighborCol >= Size)
                    {
                        // Treat out-of-bounds neighbors as walls to encourage closed borders.
                        count++;
                        continue;
                    }

                    if (maze[neighborRow][neighborCol] == Cell.Wall)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // Cellular automata can produce disconnected open regions. This carves straight
        // corridors between the centroid of each disconnected region and the start cell
        // to guarantee the maze is fully traversable.
        private static void ConnectRegions(Cell[][] maze)
        {
            bool[,] visited = new bool[Size, Size];
            var regionRepresentatives = new List<(int Row, int Col)>();

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (maze[row][col] == Cell.Empty && !visited[row, col])
                    {
                        regionRepresentatives.Add(FloodFill(maze, visited, row, col));
                    }
                }
            }

            if (regionRepresentatives.Count <= 1)
                return;

            var start = regionRepresentatives[0];
            for (int i = 1; i < regionRepresentatives.Count; i++)
            {
                CarvePath(maze, start, regionRepresentatives[i]);
            }
        }

        private static (int Row, int Col) FloodFill(Cell[][] maze, bool[,] visited, int startRow, int startCol)
        {
            var stack = new Stack<(int Row, int Col)>();
            stack.Push((startRow, startCol));
            visited[startRow, startCol] = true;
            var representative = (startRow, startCol);

            while (stack.Count > 0)
            {
                var (row, col) = stack.Pop();
                representative = (row, col);

                foreach (var (neighborRow, neighborCol) in new[]
                {
                    (row - 1, col), (row + 1, col), (row, col - 1), (row, col + 1)
                })
                {
                    if (neighborRow < 0 || neighborRow >= Size || neighborCol < 0 || neighborCol >= Size)
                        continue;

                    if (visited[neighborRow, neighborCol] || maze[neighborRow][neighborCol] == Cell.Wall)
                        continue;

                    visited[neighborRow, neighborCol] = true;
                    stack.Push((neighborRow, neighborCol));
                }
            }

            return representative;
        }

        private static void CarvePath(Cell[][] maze, (int Row, int Col) from, (int Row, int Col) to)
        {
            int row = from.Row;
            int col = from.Col;

            while (row != to.Row)
            {
                row += row < to.Row ? 1 : -1;
                maze[row][col] = Cell.Empty;
            }

            while (col != to.Col)
            {
                col += col < to.Col ? 1 : -1;
                maze[row][col] = Cell.Empty;
            }
        }
    }
}
