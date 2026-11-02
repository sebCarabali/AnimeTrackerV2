using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnimeTracker.Calendar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "calendar");

            migrationBuilder.CreateTable(
                name: "anime_releases",
                schema: "calendar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    anime_id = table.Column<int>(type: "integer", nullable: false),
                    episode_number = table.Column<int>(type: "integer", nullable: false),
                    episode_title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    air_date_time = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    is_aired = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_anime_releases", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anime_releases_air_date_time",
                schema: "calendar",
                table: "anime_releases",
                column: "air_date_time");

            migrationBuilder.CreateIndex(
                name: "ix_anime_releases_anime_id_episode_number",
                schema: "calendar",
                table: "anime_releases",
                columns: new[] { "anime_id", "episode_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anime_releases",
                schema: "calendar");
        }
    }
}
