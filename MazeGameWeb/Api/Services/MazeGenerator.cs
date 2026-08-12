using MazeGame.Api.Models;

namespace MazeGame.Api.Services
{
    // Facade around the active maze generation algorithm. Currently uses cellular automata
    // to generate cave-like mazes. See WilsonMazeGenerator for the perfect-maze alternative.
    public class MazeGenerator : IMazeGenerator
    {
        public const int FirstIndex = 0;
        public const int LastIndex = 19;
        public const int size = 20;

        private static readonly IMazeGenerator activeGenerator = new ModifiedWilsonMazeGenerator();

        Cell[][] IMazeGenerator.Generate() => Generate();

        public static Cell[][] Generate()
        {
            return activeGenerator.Generate();
        }
    }
}
