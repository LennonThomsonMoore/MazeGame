using MazeGame.Api.Models;
using System;

namespace MazeGame.Api.Contracts
{
    public class MoveResponse
    {
        public Guid GameId { get; set; }
        public PlayerType CurrentPlayer { get; set; }
        public int TurnNumber { get; set; }
        public GameStatus GameStatus { get; set; }
        public PlayerType? Winner { get; set; }

        public MoveResponse(
            Guid gameId,
            PlayerType currentPlayer,
            int turnNumber,
            GameStatus gameStatus,
            PlayerType? winner)
        {
            GameId = gameId;
            CurrentPlayer = currentPlayer;
            TurnNumber = turnNumber;
            GameStatus = gameStatus;
            Winner = winner;
        }
    }
}
