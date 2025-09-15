using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntimacyAI.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "model_performance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    model_version = table.Column<string>(type: "TEXT", nullable: true),
                    accuracy_metrics = table.Column<string>(type: "TEXT", nullable: true),
                    performance_metrics = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_performance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usage_analytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    anonymous_user_id = table.Column<string>(type: "TEXT", nullable: true),
                    feature_used = table.Column<string>(type: "TEXT", nullable: true),
                    usage_duration = table.Column<int>(type: "INTEGER", nullable: true),
                    platform = table.Column<string>(type: "TEXT", nullable: true),
                    app_version = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_analytics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "model_performance");

            migrationBuilder.DropTable(
                name: "usage_analytics");
        }
    }
}
