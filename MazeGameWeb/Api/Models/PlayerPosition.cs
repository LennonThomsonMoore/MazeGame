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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var other = (PlayerPosition)obj;
            return this.Row == other.Row && this.Column == other.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

    }
}
