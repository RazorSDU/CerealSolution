using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CerealApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cereals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mfr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Calories = table.Column<int>(type: "int", nullable: false),
                    Protein = table.Column<int>(type: "int", nullable: false),
                    Fat = table.Column<int>(type: "int", nullable: false),
                    Sodium = table.Column<int>(type: "int", nullable: false),
                    Fiber = table.Column<float>(type: "real", nullable: false),
                    Carbo = table.Column<float>(type: "real", nullable: false),
                    Sugars = table.Column<int>(type: "int", nullable: false),
                    Potass = table.Column<int>(type: "int", nullable: false),
                    Vitamins = table.Column<int>(type: "int", nullable: false),
                    Shelf = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    Cups = table.Column<float>(type: "real", nullable: false),
                    Rating = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cereals", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cereals");
        }
    }
}
