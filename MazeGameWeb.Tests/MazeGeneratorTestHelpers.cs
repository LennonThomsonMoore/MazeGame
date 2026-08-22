using MazeGame.Api.Models;

namespace MazeGameWeb.Tests;

// Shared helpers for validating properties common to all maze generators.
internal static class MazeGeneratorTestHelpers
{
    public static bool IsFullyConnected(Cell[][] maze)
    {
        int size = maze.Length;
        var visited = new bool[size, size];
        var start = FindFirstEmptyCell(maze);
        if (start is null)
        {
            return false;
        }

        var stack = new Stack<(int Row, int Col)>();
        stack.Push(start.Value);
        visited[start.Value.Row, start.Value.Col] = true;
        int visitedCount = 1;

        while (stack.Count > 0)
        {
            var (row, col) = stack.Pop();

            foreach (var (neighborRow, neighborCol) in new[]
            {
                (row - 1, col), (row + 1, col), (row, col - 1), (row, col + 1)
            })
            {
                if (neighborRow < 0 || neighborRow >= size || neighborCol < 0 || neighborCol >= size)
                    continue;

                if (visited[neighborRow, neighborCol] || maze[neighborRow][neighborCol] == Cell.Wall)
                    continue;

                visited[neighborRow, neighborCol] = true;
                visitedCount++;
                stack.Push((neighborRow, neighborCol));
            }
        }

        int totalEmpty = 0;
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                if (maze[row][col] == Cell.Empty)
                {
                    totalEmpty++;
                }
            }
        }

        return visitedCount == totalEmpty;
    }

    private static (int Row, int Col)? FindFirstEmptyCell(Cell[][] maze)
    {
        for (int row = 0; row < maze.Length; row++)
        {
            for (int col = 0; col < maze[row].Length; col++)
            {
                if (maze[row][col] == Cell.Empty)
                {
                    return (row, col);
                }
            }
        }

        return null;
    }
}
