using Autofac;
using MazeGame.Api.Models;
using MazeGameAi.Client;
using MazeGameAi.src.Agents;
using MazeGameAi.src.GameLoop;
using MazeGameAi.src.PathFinding;

namespace MazeGameAi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<RandomDirectionGenerator>().As<IRandomDirectionGenerator>().SingleInstance();
            builder.RegisterType<Dijkstra>().As<PathfindingAlgorithm>();

            builder.RegisterType<FugalAgent>().Keyed<IAgent>(PlayerType.Seeker);
            builder.RegisterType<ClustersAgent>().Keyed<IAgent>(PlayerType.Hider);

            builder.RegisterType<GameLoop>().AsSelf();

            var container = builder.Build();

            Console.WriteLine("Do you want to start a new game or join an existing one? (new/join)");
            string? choice = Console.ReadLine()?.Trim().ToLower();
            GameLoop gameLoop = container.Resolve<GameLoop>();
            if (choice == "join")
            {
                Console.WriteLine("AI will join game.");
                Console.WriteLine("What is the Game ID to join?");
                if (Guid.TryParse(Console.ReadLine()?.Trim(), out Guid gameIdInput))
                {
                    await gameLoop.Start(gameIdInput);
                }
                else
                {
                    Console.WriteLine("Invalid Game ID. Please try again.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("AI will create a new game.");
                await gameLoop.Start();
            }
            Console.WriteLine("AI has terminated.");
        }

    }
}