using System;

namespace MazeGame.Api.Contracts
{
    public class MeResponse
    {
        public string Username { get; set; }

        public MeResponse(string username)
        {
            Username = username;
        }
    }
}
