using MazeGame.Api.Models;
using System;

namespace MazeGame.Api.Contracts
{
    public class PollResponse
    {
        public Guid GameId { get; set; }
        public GameStatus Status { get; set; }
        public Cell[][]? Maze { get; set; }
        public PlayerPosition? YourPosition { get; set; }
        public PlayerPosition? OpponentPosition { get; set; }
        public PlayerType CurrentPlayer { get; set; }
        public int TurnNumber { get; set; }
        public int MovesUntilReveal { get; set; }
        public PlayerType? Winner { get; set; }

        public PollResponse(
            Guid GameId,
            PlayerPosition? YourPosition,
            PlayerPosition? OpponentPosition,
            PlayerType CurrentPlayer,
            int TurnNumber,
            int MovesUntilReveal,
            GameStatus Status,
            PlayerType? Winner,
            Cell[][]? Maze
            )
        {
            this.GameId = GameId;
            this.Status = Status;
            this.Maze = Maze;
            this.YourPosition = YourPosition;
            this.OpponentPosition = OpponentPosition;
            this.CurrentPlayer = CurrentPlayer;
            this.TurnNumber = TurnNumber;
            this.MovesUntilReveal = MovesUntilReveal;
            this.Winner = Winner;
        }

        public static PollResponse ForWaitingForPlayer(Guid gameId)
        {
            return new PollResponse(
                gameId,
                YourPosition: null,
                OpponentPosition: null,
                CurrentPlayer: default,
                TurnNumber: 0,
                MovesUntilReveal: 0,
                Status: GameStatus.WaitingForPlayer,
                Winner: null,
                Maze: null
            );
        }

        public static PollResponse ForGameOver(Guid gameId, PlayerType winner, PlayerPosition? yourPosition, PlayerPosition? opponentPosition, Cell[][]? maze)
        {
            return new PollResponse(
                gameId,
                YourPosition: yourPosition,
                OpponentPosition: opponentPosition,
                CurrentPlayer: default,
                TurnNumber: 0,
                MovesUntilReveal: 0,
                Status: GameStatus.Completed,
                Winner: winner,
                Maze: maze
            );
        }
    }
}
