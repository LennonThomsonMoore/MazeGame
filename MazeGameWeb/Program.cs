using MazeGame.Api.Data;
using MazeGame.Api.Endpoints;
using MazeGame.Api.Services;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static MazeGame.Api.Endpoints.Login;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDbContext>(options => 
    options.UseSqlite("Data Source=Api/Data/maze.db"));

builder.Services.AddDbContext<UserDbContext>(options => 
    options.UseSqlite("Data Source=Api/Data/user.db"));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ITokenService, TokenService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key configuration value is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token) &&
                    context.Request.Cookies.TryGetValue(AuthCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<GameDbContext>()
        .Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<UserDbContext>()
        .Database.MigrateAsync();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Creates mappings to all endpoints in the Api/Endpoints folder
app.MapCreateGameEndpoint();
app.MapJoinLobbyEndpoint();
app.MapMoveEndpoint();
app.MapPollEndpoint();
app.MapWaitingGames();
app.MapRegisterEndpoint();
app.MapLoginEndpoints();
app.MapLogoutEndpoint();
app.MapMeEndpoint();
app.MapListMeGamesEndpoint();

app.Run();
