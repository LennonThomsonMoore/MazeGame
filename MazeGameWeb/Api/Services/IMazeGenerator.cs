using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    public interface IMazeGenerator
    {
        Cell[][] Generate();
    }
}
