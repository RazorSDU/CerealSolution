using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CerealApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Cereals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Cereals");
        }
    }
}
