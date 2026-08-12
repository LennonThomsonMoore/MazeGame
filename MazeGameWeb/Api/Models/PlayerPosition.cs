using System;
using System.Collections.Generic;
using System.Text;

namespace MazeGame.Api.Models
{
    public class PlayerPosition
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public PlayerPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool Equals(PlayerPosition other)
        {
            if (other == null) return false;
            return this.Row == other.Row && this.Column == other.Column;
        }
    }
}
