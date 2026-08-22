using MazeGame.Api.Models;
using MazeGame.Api.Services;

namespace MazeGameWeb.Tests;

public class WilsonMazeGeneratorTests
{
    [Fact]
    public void Generate_ReturnsMazeWithExpectedDimensions()
    {
        var generator = new WilsonMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.Equal(Maze.size, maze.Length);
        Assert.All(maze, row => Assert.Equal(Maze.size, row.Length));
    }

    [Fact]
    public void Generate_OpensStartAndEndCorners()
    {
        var generator = new WilsonMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.Equal(Cell.Empty, maze[0][0]);
        Assert.Equal(Cell.Empty, maze[Maze.LastIndex][Maze.LastIndex]);
        Assert.Equal(Cell.Empty, maze[Maze.LastIndex - 1][Maze.LastIndex]);
    }

    [Fact]
    public void Generate_ProducesFullyConnectedMaze()
    {
        var generator = new WilsonMazeGenerator();

        Cell[][] maze = generator.Generate();

        Assert.True(MazeGeneratorTestHelpers.IsFullyConnected(maze));
    }

    [Fact]
    public void Generate_LogicalCellsAreOpen()
    {
        var generator = new WilsonMazeGenerator();

        Cell[][] maze = generator.Generate();

        for (int row = 0; row < Maze.size; row += 2)
        {
            for (int col = 0; col < Maze.size; col += 2)
            {
                Assert.Equal(Cell.Empty, maze[row][col]);
            }
        }
    }

    [Fact]
    public void Generate_CalledMultipleTimes_AlwaysProducesConnectedMaze()
    {
        var generator = new WilsonMazeGenerator();

        for (int i = 0; i < 5; i++)
        {
            Cell[][] maze = generator.Generate();
            Assert.True(MazeGeneratorTestHelpers.IsFullyConnected(maze));
        }
    }
}
