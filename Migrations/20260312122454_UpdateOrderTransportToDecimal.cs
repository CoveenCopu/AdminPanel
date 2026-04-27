using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminPanel.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderTransportToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YearSummaries");

            migrationBuilder.AlterColumn<decimal>(
                name: "Transport",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Transport",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "YearSummaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AntalJobs = table.Column<int>(type: "int", nullable: false),
                    Lønudgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Omsætning = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SamletUdgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Udgifter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    År = table.Column<int>(type: "int", nullable: false),
                    Årsopgørelse = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YearSummaries", x => x.Id);
                });
        }
    }
}
