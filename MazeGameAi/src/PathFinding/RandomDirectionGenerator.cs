using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeGame.Api.Models;

namespace MazeGameAi.src.PathFinding
{
    public class RandomDirectionGenerator : IRandomDirectionGenerator
    {
        public Direction generate()
        {
            return (Direction)new Random().Next(0, 4);
        }
    }
}
