using MazeGame.Api.Data;
using MazeGame.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDbContext>(options => 
    options.UseSqlite("Data Source=Api/Data/maze.db"));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    await db.Database.MigrateAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Creates mappings to all endpoints in the Api/Endpoints folder
app.MapCreateGameEndpoint();
app.MapJoinLobbyEndpoint();
app.MapMoveEndpoint();
app.MapPollEndpoint();
app.MapWaitingGames();

app.Run();
