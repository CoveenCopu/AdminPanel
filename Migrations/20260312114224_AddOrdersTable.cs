using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminPanel.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kunde = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    By = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Opsætningsdato = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MEDtagningsdato = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Pris = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Betalingsdato = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Transport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salgsmoms = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Noter = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
