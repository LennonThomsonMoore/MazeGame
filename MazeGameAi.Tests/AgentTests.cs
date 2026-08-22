using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeGame.Api.Contracts;
using MazeGame.Api.Models;
using MazeGameAi.src.Agents;
using MazeGameAi.src.PathFinding;
using Moq;
using Xunit;

namespace MazeGameAi.Tests
{
    public class AgentTests
    {
        private static Cell[][] CreateMaze()
        {
            return new Cell[3][]
            {
                new[] { Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty },
                new[] { Cell.Empty, Cell.Empty, Cell.Empty },
            };
        }

        private static PollResponse CreateGameState(PlayerPosition yourPosition, PlayerPosition? opponentPosition)
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
                CreateMaze());
        }

        [Fact]
        public void PetalAgent_CallsNextMoveTowardsTarget()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            pathfindingMock
                .Setup(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()))
                .Returns(Direction.North);

            var agent = new PetalAgent(pathfindingMock.Object);
            var yourPosition = new PlayerPosition(1, 1);
            var opponentPosition = new PlayerPosition(1, 2);
            var gameState = CreateGameState(yourPosition, opponentPosition);

            Direction result = agent.decideMove(gameState);

            pathfindingMock.Verify(p => p.NextMoveTowardsTarget(gameState.Maze, yourPosition, opponentPosition, null), Times.Once);
            pathfindingMock.Verify(p => p.NextMoveAwayFromTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()), Times.Never);
            Assert.Equal(Direction.North, result);
        }

        [Fact]
        public void FugalAgent_CallsNextMoveAwayFromTarget()
        {
            var pathfindingMock = new Mock<PathfindingAlgorithm>();
            pathfindingMock
                .Setup(p => p.NextMoveAwayFromTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()))
                .Returns(Direction.South);

            var agent = new FugalAgent(pathfindingMock.Object);
            var yourPosition = new PlayerPosition(1, 1);
            var opponentPosition = new PlayerPosition(1, 2);
            var gameState = CreateGameState(yourPosition, opponentPosition);

            Direction result = agent.decideMove(gameState);

            pathfindingMock.Verify(p => p.NextMoveAwayFromTarget(gameState.Maze, yourPosition, opponentPosition, null), Times.Once);
            pathfindingMock.Verify(p => p.NextMoveTowardsTarget(It.IsAny<Cell[][]>(), It.IsAny<PlayerPosition>(), It.IsAny<PlayerPosition?>(), It.IsAny<IEnumerable<PlayerPosition>?>()), Times.Never);
            Assert.Equal(Direction.South, result);
        }
    }
}
