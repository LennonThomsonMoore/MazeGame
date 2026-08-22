using System;
using System.Collections.Generic;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using MazeGameAi.src.Agents;
using MazeGameAi.src.PathFinding;
using Moq;
using Xunit;

namespace MazeGameAi.Tests
{
    public class ClustersAgentTests
    {
        private static Cell[][] CreateMaze()
        {
            return new Cell[5][]
            {
                new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty, Cell.Empty },
            };
        }

        private static PollResponse CreateGameState(PlayerPosition yourPosition, PlayerPosition? opponentPosition, Cell[][]? maze = null)
        {
            return new PollResponse(
                Guid.NewGuid(),
                yourPosition,
                opponentPosition,
                PlayerType.Hider,
                1,
                1,
                GameStatus.Active,
                null,
                maze ?? CreateMaze());
        }

        [Fact]
        public void ClustersAgent_CallsNextMoveTowardsTarget()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            pathfindingMock
                .Setup(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()))
                .Returns(Direction.North);

            var agent = new ClustersAgent(pathfindingMock.Object);
            var yourPosition = new PlayerPosition(0, 0);
            var opponentPosition = new PlayerPosition(4, 4);
            var gameState = CreateGameState(yourPosition, opponentPosition);

            Direction result = agent.decideMove(gameState);

            pathfindingMock.Verify(p => p.NextMoveTowardsTarget(
                gameState.Maze,
                yourPosition,
                It.IsAny<PlayerPosition?>(),
                It.IsAny<IEnumerable<PlayerPosition>?>()), Times.Once);
            Assert.Equal(Direction.North, result);
        }

        [Fact]
        public void ClustersAgent_ReturnsRandomDirection_WhenMazeIsNull()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            var agent = new ClustersAgent(pathfindingMock.Object);
            var gameState = new PollResponse(
                Guid.NewGuid(),
                new PlayerPosition(0, 0),
                null,
                PlayerType.Hider,
                1,
                1,
                GameStatus.Active,
                null,
                null);

            Direction result = agent.decideMove(gameState);

            Assert.True(Enum.IsDefined(typeof(Direction), result));
            pathfindingMock.Verify(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()), Times.Never);
        }

        [Fact]
        public void ClustersAgent_ReturnsRandomDirection_WhenYourPositionIsNull()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            var agent = new ClustersAgent(pathfindingMock.Object);
            var gameState = new PollResponse(
                Guid.NewGuid(),
                null,
                new PlayerPosition(1, 1),
                PlayerType.Hider,
                1,
                1,
                GameStatus.Active,
                null,
                CreateMaze());

            Direction result = agent.decideMove(gameState);

            Assert.True(Enum.IsDefined(typeof(Direction), result));
            pathfindingMock.Verify(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()), Times.Never);
        }

        [Fact]
        public void ClustersAgent_UsesLastSeenOpponentPosition_WhenOpponentBecomesNull()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            pathfindingMock
                .Setup(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()))
                .Returns(Direction.East);

            var agent = new ClustersAgent(pathfindingMock.Object);
            var yourPosition = new PlayerPosition(0, 0);
            var opponentPosition = new PlayerPosition(4, 4);
            var maze = CreateMaze();

            // First call establishes the last seen opponent position and initializes clusters.
            agent.decideMove(CreateGameState(yourPosition, opponentPosition, maze));

            // Second call: opponent position is now unknown (null), agent should still behave correctly.
            Direction result = agent.decideMove(CreateGameState(yourPosition, null, maze));

            Assert.Equal(Direction.East, result);
        }
    }
}
