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

        public static PollResponse ForWaitingForPlayer(Guid gameId, GameStatus status)
        {
            return new PollResponse(
                gameId,
                YourPosition: null,
                OpponentPosition: null,
                CurrentPlayer: default,
                TurnNumber: 0,
                MovesUntilReveal: 0,
                Status: status,
                Winner: null,
                Maze: null
            );
        }

        public static PollResponse ForGameOver(Guid gameId, GameStatus status, PlayerType winner)
        {
            return new PollResponse(
                gameId,
                YourPosition: null,
                OpponentPosition: null,
                CurrentPlayer: default,
                TurnNumber: 0,
                MovesUntilReveal: 0,
                Status: status,
                Winner: winner,
                Maze: null
            );
        }
    }
}
