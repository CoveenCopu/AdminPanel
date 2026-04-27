using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminPanel.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Produkt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dato = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Antal = table.Column<int>(type: "int", nullable: false),
                    PrisPrStk = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPris = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Noter = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YearSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    År = table.Column<int>(type: "int", nullable: false),
                    Omsætning = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Udgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Lønudgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SamletUdgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AntalJobs = table.Column<int>(type: "int", nullable: false),
                    Årsopgørelse = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearSummaries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "YearSummaries");
        }
    }
}
