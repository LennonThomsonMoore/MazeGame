namespace WB.Tests;

using MazeGameAi.src.PathFinding;
using MazeGame.Api.Models;
using Moq;

public class DijkstraTests
{
    private readonly Mock<IRandomDirectionGenerator> directionGeneratorMock = new Mock<IRandomDirectionGenerator>();
    private readonly Dijkstra dijkstra;

    public DijkstraTests()
    {
        directionGeneratorMock.Setup(g => g.generate()).Returns(Direction.North);
        dijkstra = new Dijkstra(directionGeneratorMock.Object);
    }
    [Fact]
    public void GoesDirectToOpponent()
    {
        Cell[][] maze = new Cell[3][]
        {
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
        };

        var yourPosition = new PlayerPosition(1, 1);
        var opponentPosition = new PlayerPosition(1, 2);

        Direction result = dijkstra.NextMoveTowardsTarget(maze, yourPosition, opponentPosition);

        Assert.Equal(Direction.East, result);
    }
    [Fact]
    public void GoesAwayFromOpponentAndAvoidsWalls()
    {
        Cell[][] maze = new Cell[3][]
        {
            new[] { Cell.Empty, Cell.Wall, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Wall, Cell.Empty },
        };

        var yourPosition = new PlayerPosition(1, 1);
        var opponentPosition = new PlayerPosition(1, 2);

        Direction result = dijkstra.NextMoveAwayFromTarget(maze, yourPosition, opponentPosition);

        Assert.Equal(Direction.West, result);
    }
    [Fact]
    public void ReturnsRandomDirectionWhenNoTarget()
    {
        Cell[][] maze = new Cell[3][]
        {
            new[] { Cell.Empty, Cell.Wall, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Wall, Cell.Empty },
        };
        var yourPosition = new PlayerPosition(1, 1);
        Direction result = dijkstra.NextMoveTowardsTarget(maze, yourPosition, null);
        Assert.Equal(result, Direction.North);
    }

    [Fact]
    public void AvoidsSpecifiedPositions()
    {
        Cell[][] maze = new Cell[3][]
        {
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            new[] { Cell.Empty, Cell.Empty, Cell.Empty },
        };
        var yourPosition = new PlayerPosition(1, 1);
        var opponentPosition = new PlayerPosition(1, 2);
        var positionsToAvoid = new List<PlayerPosition> { new PlayerPosition(1, 0), new PlayerPosition(0, 1)};
        Direction result = dijkstra.NextMoveAwayFromTarget(maze, yourPosition, opponentPosition, positionsToAvoid);
        Assert.Equal(Direction.South, result);
    }
}
