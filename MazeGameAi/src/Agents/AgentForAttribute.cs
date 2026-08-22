using MazeGame.Api.Models;
using System;

namespace MazeGameAi.src.Agents
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AgentForAttribute : Attribute
    {
        public PlayerType PlayerType { get; }

        public AgentForAttribute(PlayerType playerType)
        {
            PlayerType = playerType;
        }
    }
}
