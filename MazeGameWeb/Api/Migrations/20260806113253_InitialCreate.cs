using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MazeGameWeb.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Maze = table.Column<string>(type: "TEXT", nullable: true),
                    HiderPosition_Row = table.Column<int>(type: "INTEGER", nullable: true),
                    HiderPosition_Column = table.Column<int>(type: "INTEGER", nullable: true),
                    SeekerPosition_Row = table.Column<int>(type: "INTEGER", nullable: true),
                    SeekerPosition_Column = table.Column<int>(type: "INTEGER", nullable: true),
                    CurrentPlayer = table.Column<int>(type: "INTEGER", nullable: false),
                    TurnNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Winner = table.Column<int>(type: "INTEGER", nullable: true),
                    HiderToken = table.Column<Guid>(type: "TEXT", nullable: true),
                    SeekerToken = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
